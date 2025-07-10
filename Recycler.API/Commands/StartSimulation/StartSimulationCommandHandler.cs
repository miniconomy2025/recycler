using MediatR;
using Recycler.API.Services;
using Recycler.API.Commands;
using Recycler.API.Utils;
using Recycler.API.Commands.CreatePickupRequest;

namespace Recycler.API.Commands.StartSimulation;

public class StartSimulationCommandHandler : IRequestHandler<StartSimulationCommand, StartSimulationResponse>
{
    private readonly HttpClient _http;
    private readonly ISimulationClock _clock;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly MakePaymentService _paymentService;
    private readonly ICommercialBankService _commercialBankService;

    public StartSimulationCommandHandler(
        IHttpClientFactory httpFactory,
        ISimulationClock clock,
        IMediator mediator,
        IConfiguration configuration,
        MakePaymentService paymentService, 
        ICommercialBankService commercialBankService)
    {
        _http = httpFactory.CreateClient();
        _clock = clock;
        _mediator = mediator;
        _configuration = configuration;
        _paymentService = paymentService;
        _commercialBankService = commercialBankService;

        var bankUrl = _configuration["commercialBankUrl"] ?? "http://localhost:8085";
        _http.BaseAddress = new Uri(bankUrl);
    }

    public async Task<StartSimulationResponse> Handle(StartSimulationCommand request, CancellationToken cancellationToken)
    {
        DateTime? realStart = request.EpochStartTime.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(request.EpochStartTime.Value).UtcDateTime
            : null;

        _clock.Start(realStart);

        var notificationUrl = $"{_configuration["recyclerApi:baseUrl"]}{_configuration["recyclerApi:bankNotificationPath"]}";

        var accountResponse = await RetryHelper.RetryAsync(
            () => _http.PostAsJsonAsync("/account", new
            {
                notification_url = notificationUrl
            }, cancellationToken),
            maxAttempts: 20,
            operationName: "Create bank account");
        if (!accountResponse.IsSuccessStatusCode)
            return new StartSimulationResponse { Status = "error", Message = "Failed to create bank account" };

        var accountData = await accountResponse.Content.ReadFromJsonAsync<AccountResponse>(cancellationToken: cancellationToken);
        if (accountData is null)
            return new StartSimulationResponse { Status = "error", Message = "Invalid bank account response" };

        _commercialBankService.AccountNumber = accountData.account_number;

        var thoHUrl = _configuration["thoHApiUrl"] ?? "http://localhost:8084";
        var machinesResponse = await RetryHelper.RetryAsync(
            () => _http.GetAsync($"{thoHUrl}/machines", cancellationToken),
            operationName: "Fetch machines from THoH");
        if (!machinesResponse.IsSuccessStatusCode)
            return new StartSimulationResponse { Status = "error", Message = "Could not retrieve machine list" };

        var machineMarket = await machinesResponse.Content.ReadFromJsonAsync<MachineMarketResponse>(cancellationToken: cancellationToken);
        if (machineMarket == null || machineMarket.machines.Count == 0)
            return new StartSimulationResponse { Status = "error", Message = "No machines available" };

        var recyclingMachine = machineMarket.machines.FirstOrDefault(m => m.machineName == "recycling_machine");
        if (recyclingMachine == null)
            return new StartSimulationResponse { Status = "error", Message = "Recycling machine not found" };

        var totalCost = recyclingMachine.price * 2;
        var loanResponse = await RetryHelper.RetryAsync(
            () => _http.PostAsJsonAsync("/loan", new { amount = totalCost + 5000 }, cancellationToken),
            operationName: "Request loan");
        if (!loanResponse.IsSuccessStatusCode)
            return new StartSimulationResponse { Status = "error", Message = "Loan request failed" };

        var loanData = await loanResponse.Content.ReadFromJsonAsync<LoanResponse>(cancellationToken: cancellationToken);

        var orderCommand = new PlaceMachineOrderCommand
        {
            machineName = recyclingMachine.machineName,
            quantity = 2
        };

        string? orderNumber = null;
        string? thoHAccount = null;
        try
        {
            var orderResult = await RetryHelper.RetryAsync(
                () => _mediator.Send(orderCommand, cancellationToken),
                operationName: "Place machine order");
            orderNumber = orderResult.OrderId.ToString();
            thoHAccount = orderResult.BankAccount;
            Console.WriteLine($"Ordered recycling machine: {orderResult.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to order recycling machine: {ex.Message}");
        }

        if (string.IsNullOrEmpty(thoHAccount))
        {
            return new StartSimulationResponse { Status = "error", Message = "Missing THoH account number config" };
        }

        if (!string.IsNullOrEmpty(orderNumber))
        {
            try
            {
                var paymentResult = await RetryHelper.RetryAsync(
                    () => _paymentService.SendPaymentAsync(
                        toAccountNumber: thoHAccount!,
                        amount: totalCost,
                        description: orderNumber,
                        cancellationToken),
                    operationName: "Send machine payment");

                Console.WriteLine($"Payment made: Tx#{paymentResult.transaction_number}");
            }
            catch (Exception ex)
            {
                return new StartSimulationResponse { Status = "error", Message = $"Machine payment failed: {ex.Message}" };
            }
        }
        
        var simTime = _clock.GetCurrentSimulationTime();

        return new StartSimulationResponse
        {
            Status = "started",
            Message = $"Simulation started with loan #{loanData?.loan_number ?? "unknown"} at {simTime:yyyy-MM-dd HH:mm:ss}, 2 recycling machines ordered and paid"
        };
    }

    private class AccountResponse
    {
        public string account_number { get; set; } = default!;
    }

    private class LoanResponse
    {
        public string loan_number { get; set; } = default!;
        public bool success { get; set; }
    }

    private class MachineMarketResponse
    {
        public List<MachineDto> machines { get; set; } = new();
    }

    private class MachineDto
    {
        public string machineName { get; set; } = default!;
        public int quantity { get; set; }
        public string materialRatio { get; set; } = default!;
        public int productionRate { get; set; }
        public decimal price { get; set; }
    }
}
