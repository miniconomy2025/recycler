using Recycler.API.Utils;

namespace Recycler.API.Services;

public class LoanService
{
    private readonly HttpClient _http;
    private readonly ILogger<LoanService> _logger;

    public LoanService(IHttpClientFactory factory, IConfiguration config, ILogger<LoanService> logger)
    {
        _http = factory.CreateClient("test");
        var bankUrl = config["commercialBankUrl"] ?? "http://localhost:8085";
        _http.BaseAddress = new Uri(bankUrl);
        _logger = logger;
    }

    public async Task<LoanResponse?> RequestLoanAsync(decimal initialAmount, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requesting loan for amount: {InitialAmount}", initialAmount);
        
        LoanResponse? loanData = null;
        decimal loanAmount = initialAmount;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            _logger.LogInformation("Loan attempt {AttemptNumber}/2 for amount: {LoanAmount}", attempt + 1, loanAmount);
            
            var response = await RetryHelper.RetryAsync(
                () => _http.PostAsJsonAsync("api/loan", new { amount = loanAmount }, cancellationToken),
                operationName: $"Loan attempt {attempt + 1}");

            loanData = await response.Content.ReadFromJsonAsync<LoanResponse>(cancellationToken: cancellationToken);

            if (loanData == null)
            {
                _logger.LogError("Loan response is null on attempt {AttemptNumber}", attempt + 1);
                return null;
            }

            _logger.LogInformation("Loan response - Success: {Success}, Loan Number: {LoanNumber}, Amount Remaining: {AmountRemaining}", 
                loanData.success, loanData.loan_number, loanData.amount_remaining);

            if (loanData.success)
            {
                _logger.LogInformation("Loan approved successfully on attempt {AttemptNumber}", attempt + 1);
                return loanData;
            }

            if (loanData.amount_remaining > 0)
            {
                _logger.LogInformation("Loan partially approved, retrying with remaining amount: {AmountRemaining}", loanData.amount_remaining);
                loanAmount = loanData.amount_remaining;
                continue;
            }

            _logger.LogWarning("Loan request failed on attempt {AttemptNumber} - no amount remaining", attempt + 1);
            break;
        }

        _logger.LogWarning("All loan attempts exhausted. Final result - Success: {Success}, Loan Number: {LoanNumber}", 
            loanData?.success, loanData?.loan_number);
        return loanData;
    }

    public class LoanResponse
    {
        public string loan_number { get; set; } = default!;
        public bool success { get; set; }
        public decimal amount_remaining { get; set; }
    }
}
