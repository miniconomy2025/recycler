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
    private readonly ILogger<SimulationBootstrapService> _logger;

    public SimulationBootstrapService(
        IConfiguration config,
        ICommercialBankService bankService,
        IMediator mediator,
        MakePaymentService paymentService,
        BankAccountService accountService,
        LoanService loanService,
        MachineMarketService marketService,
        ILogger<SimulationBootstrapService> logger)
    {
        _config = config;
        _bankService = bankService;
        _mediator = mediator;
        _paymentService = paymentService;
        _accountService = accountService;
        _loanService = loanService;
        _marketService = marketService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting simulation bootstrap process");
        
        try
        {
            _logger.LogInformation("Step 1: Registering bank account with notification URL");
            var notificationUrl = $"{_config["recyclerApi:baseUrl"]}{_config["recyclerApi:bankNotificationPath"]}";
            _logger.LogInformation("Notification URL configured: {NotificationUrl}", notificationUrl);
            
            var accountNumber = await _accountService.RegisterAsync(notificationUrl, cancellationToken);
            if (accountNumber == null) 
            {
                _logger.LogError("Failed to register bank account - received null account number");
                throw new Exception("Failed to register bank account");
            }
            
            _logger.LogInformation("Bank account registered successfully: {AccountNumber}", accountNumber);
            _bankService.AccountNumber = accountNumber;

            _logger.LogInformation("Step 2: Fetching available recycling machines from market");
            var machine = await _marketService.GetRecyclingMachineAsync(cancellationToken);
            if (machine == null) 
            {
                _logger.LogError("No recycling machines available in market");
                throw new Exception("No recycling machines available");
            }
            
            _logger.LogInformation("Found recycling machine: {MachineName}, Price: {Price}, Production Rate: {ProductionRate}", 
                machine.machineName, machine.price, machine.productionRate);

            var totalCost = machine.price * 2;
            var loanAmount = totalCost + 10000;
            _logger.LogInformation("Step 3: Calculating costs - Machine cost: {MachineCost}, Total cost for 2 machines: {TotalCost}, Loan amount: {LoanAmount}", 
                machine.price, totalCost, loanAmount);
            
            var loan = await _loanService.RequestLoanAsync(loanAmount, cancellationToken);
            if (loan == null || !loan.success) 
            {
                _logger.LogError("Loan request failed - Loan: {LoanNumber}, Success: {Success}, Amount Remaining: {AmountRemaining}", 
                    loan?.loan_number, loan?.success, loan?.amount_remaining);
                throw new Exception("Loan request failed");
            }
            
            _logger.LogInformation("Loan approved successfully: {LoanNumber}, Amount: {LoanAmount}", loan.loan_number, loanAmount);

            _logger.LogInformation("Step 4: Placing machine order - Machine: {MachineName}, Quantity: 2", machine.machineName);
            var order = await _mediator.Send(new PlaceMachineOrderCommand
            {
                machineName = machine.machineName,
                quantity = 2
            }, cancellationToken);
            
            _logger.LogInformation("Machine order placed successfully - Order ID: {OrderId}, Bank Account: {BankAccount}", 
                order.OrderId, order.BankAccount);

            _logger.LogInformation("Step 5: Processing payment - Amount: {Amount}, Description: {Description}", 
                totalCost, order.OrderId.ToString());
            
            var payment = await _paymentService.SendPaymentAsync(
                toAccountNumber: order.BankAccount ?? "",
                amount: totalCost,
                description: order.OrderId.ToString(),
                cancellationToken: cancellationToken);
            
            _logger.LogInformation("Payment processed successfully - Transaction: {TransactionNumber}", payment.transaction_number);

            _logger.LogInformation("Simulation bootstrap completed successfully - Loan: {LoanNumber}, Transaction: {TransactionNumber}", 
                loan.loan_number, payment.transaction_number);
            Console.WriteLine($"Bootstrapped simulation. Loan: {loan.loan_number}, Tx: {payment.transaction_number}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Simulation bootstrap failed: {ErrorMessage}", ex.Message);
            Console.WriteLine($"Bootstrap failed: {ex.Message}");
            throw;
        }
    }
}