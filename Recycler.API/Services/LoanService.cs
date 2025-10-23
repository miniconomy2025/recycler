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

    public async Task<LoanResponse?> RequestLoanAsync(
        decimal initialAmount, 
        CancellationToken cancellationToken,
        decimal minimumAmount = 0)
    {
        _logger.LogInformation(
            "Requesting loan for amount: {InitialAmount} (minimum allowed: {MinimumAmount})", 
            initialAmount, minimumAmount);

        LoanResponse? loanData = null;
        decimal loanAmount = initialAmount;

        const int maxAttempts = 5;
        const decimal reductionFactor = 0.8m; 

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            _logger.LogInformation(
                "Loan attempt {AttemptNumber}/{MaxAttempts} for amount: {LoanAmount}", 
                attempt + 1, maxAttempts, loanAmount);

            var response = await RetryHelper.RetryAsync(
                () => _http.PostAsJsonAsync("api/loan", new { amount = loanAmount }, cancellationToken),
                operationName: $"Loan attempt {attempt + 1}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Loan API call failed with status code: {StatusCode} on attempt {AttemptNumber}", 
                    response.StatusCode, attempt + 1);
                continue;
            }

            loanData = await response.Content.ReadFromJsonAsync<LoanResponse>(cancellationToken: cancellationToken);

            if (loanData == null)
            {
                _logger.LogError("Loan response is null on attempt {AttemptNumber}", attempt + 1);
                return null;
            }
            _logger.LogInformation(
                "Loan response - Success: {Success}, Loan Number: {LoanNumber}, Amount Remaining: {AmountRemaining}",
                loanData.success, loanData.loan_number, loanData.amount_remaining);

            if (loanData.success)
            {
                _logger.LogInformation("Loan approved successfully on attempt {AttemptNumber}", attempt + 1);
                return loanData;
            }

            if (loanData.amount_remaining > 0)
            {
                _logger.LogInformation(
                    "Loan partially approved, retrying with remaining amount: {AmountRemaining}", 
                    loanData.amount_remaining);
                loanAmount = loanData.amount_remaining;
                continue;
            }

            var newAmount = Math.Round(loanAmount * reductionFactor, 2);
            if (newAmount < minimumAmount)
            {
                _logger.LogWarning(
                    "Loan declined and reduced below minimum threshold ({MinimumAmount}). Clamping to {MinimumAmount}.", 
                    minimumAmount, minimumAmount);
                newAmount = minimumAmount;
            }

            loanAmount = newAmount;
            _logger.LogWarning("Loan declined. Reducing request by 20%, New loan amount: {NewAmount}", loanAmount);

            if (loanAmount <= minimumAmount)
            {
                _logger.LogWarning("Loan amount reached minimum threshold ({MinimumAmount}). Stopping further retries.", minimumAmount);
                break;
            }
        }

        _logger.LogWarning(
            "All loan attempts exhausted. Final result - Success: {Success}, Loan Number: {LoanNumber}",
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
