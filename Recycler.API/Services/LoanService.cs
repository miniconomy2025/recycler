using Recycler.API.Utils;

namespace Recycler.API.Services;

public class LoanService
{
    private readonly HttpClient _http;

    public LoanService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("test");
        var bankUrl = config["commercialBankUrl"] ?? "http://localhost:8085";
        _http.BaseAddress = new Uri(bankUrl);
    }

    public async Task<LoanResponse?> RequestLoanAsync(decimal initialAmount, CancellationToken cancellationToken)
    {
        LoanResponse? loanData = null;
        decimal loanAmount = initialAmount;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            var response = await RetryHelper.RetryAsync(
                () => _http.PostAsJsonAsync("/api/loan", new { amount = loanAmount }, cancellationToken),
                operationName: $"Loan attempt {attempt + 1}");

            loanData = await response.Content.ReadFromJsonAsync<LoanResponse>(cancellationToken: cancellationToken);

            if (loanData == null)
                return null;

            if (loanData.success)
                return loanData;

            if (loanData.amount_remaining > 0)
            {
                loanAmount = loanData.amount_remaining;
                continue;
            }

            break;
        }

        return loanData;
    }

    public class LoanResponse
    {
        public string loan_number { get; set; } = default!;
        public bool success { get; set; }
        public decimal amount_remaining { get; set; }
    }
}
