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

    public SimulationBootstrapService(
        IConfiguration config,
        ICommercialBankService bankService,
        IMediator mediator,
        MakePaymentService paymentService,
        BankAccountService accountService,
        LoanService loanService,
        MachineMarketService marketService)
    {
        _config = config;
        _bankService = bankService;
        _mediator = mediator;
        _paymentService = paymentService;
        _accountService = accountService;
        _loanService = loanService;
        _marketService = marketService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting simulation bootstrap process");
        
        try
        {
            Console.WriteLine("Step 1: Registering bank account with notification URL");
            var notificationUrl = $"{_config["recyclerApi:baseUrl"]}{_config["recyclerApi:bankNotificationPath"]}";
            Console.WriteLine($"Notification URL configured: {notificationUrl}");
            
            var accountNumber = await _accountService.RegisterAsync(notificationUrl, cancellationToken);
            if (accountNumber == null) throw new Exception("Failed to register bank account");
            Console.WriteLine($"Bank account registered successfully: {accountNumber}");

            _bankService.AccountNumber = accountNumber;

            Console.WriteLine("Step 2: Fetching available recycling machines from market");
            var machine = await _marketService.GetRecyclingMachineAsync(cancellationToken);
            if (machine == null) throw new Exception("No recycling machines available");
            Console.WriteLine($"Found recycling machine: {machine.machineName}, Price: {machine.price}, Production Rate: {machine.productionRate}");

            var totalCost = machine.price * 2;
            var loanAmount = totalCost + 10000;
            Console.WriteLine($"Step 3: Calculating costs - Machine cost: {machine.price}, Total cost for 2 machines: {totalCost}, Loan amount: {loanAmount}");
            
            var loan = await _loanService.RequestLoanAsync(loanAmount, cancellationToken);
            if (loan == null || !loan.success) throw new Exception("Loan request failed");
            Console.WriteLine($"Loan approved successfully: {loan.loan_number}, Amount: {loanAmount}");

            Console.WriteLine($"Step 4: Placing machine order - Machine: {machine.machineName}, Quantity: 2");
            var order = await _mediator.Send(new PlaceMachineOrderCommand
            {
                machineName = machine.machineName,
                quantity = 2
            }, cancellationToken);
            Console.WriteLine($"Machine order placed successfully - Order ID: {order.OrderId}, Bank Account: {order.BankAccount}");

            Console.WriteLine($"Step 5: Processing payment - Amount: {totalCost}, Description: {order.OrderId}");
            var payment = await _paymentService.SendPaymentAsync(
                toAccountNumber: order.BankAccount ?? "",
                amount: totalCost,
                description: order.OrderId.ToString(),
                cancellationToken: cancellationToken);
            Console.WriteLine($"Payment processed successfully - Transaction: {payment.transaction_number}");

            Console.WriteLine($"Simulation bootstrap completed successfully - Loan: {loan.loan_number}, Transaction: {payment.transaction_number}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bootstrap failed: {ex.Message}");
        }
    }
}