using Recycler.API.Commands;
using MediatR;

namespace Recycler.API.Services;

public class SimulationBootstrapService :  ISimulationBootstrapService
{
    private readonly IConfiguration _config;
    private readonly ICommercialBankService _bankService;
    private readonly IMediator _mediator;
    private readonly MakePaymentService _paymentService;
    private readonly BankAccountService _accountService;
    private readonly LoanService _loanService;
    private readonly MachineMarketService _marketService;
    private readonly ISimulationBootstrapService _bootstrapService;

    public SimulationBootstrapService(
        IConfiguration config,
        ICommercialBankService bankService,
        IMediator mediator,
        MakePaymentService paymentService,
        BankAccountService accountService,
        LoanService loanService,
        MachineMarketService marketService,
        ISimulationBootstrapService bootstrapService)
    {
        _config = config;
        _bankService = bankService;
        _mediator = mediator;
        _paymentService = paymentService;
        _accountService = accountService;
        _loanService = loanService;
        _marketService = marketService;
        _bootstrapService = bootstrapService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var notificationUrl = $"{_config["recyclerApi:baseUrl"]}{_config["recyclerApi:bankNotificationPath"]}";
            var accountNumber = await _accountService.RegisterAsync(notificationUrl, cancellationToken);
            if (accountNumber == null) throw new Exception("Failed to register bank account");

            _bankService.AccountNumber = accountNumber;

            var machine = await _marketService.GetRecyclingMachineAsync(cancellationToken);
            if (machine == null) throw new Exception("No recycling machines available");

            var totalCost = machine.price * 2;
            var loan = await _loanService.RequestLoanAsync(totalCost + 10000, cancellationToken);
            if (loan == null || !loan.success) throw new Exception("Loan request failed");

            var order = await _mediator.Send(new PlaceMachineOrderCommand
            {
                machineName = machine.machineName,
                quantity = 2
            }, cancellationToken);

            var payment = await _paymentService.SendPaymentAsync(
                toAccountNumber: order.BankAccount ?? "",
                amount: totalCost,
                description: order.OrderId.ToString(),
                cancellationToken: cancellationToken);

            Console.WriteLine($"Bootstrapped simulation. Loan: {loan.loan_number}, Tx: {payment.transaction_number}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bootstrap failed: {ex.Message}");
        }
    }
}