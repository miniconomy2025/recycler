using Recycler.API.Utils;

public class MakePaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public MakePaymentService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<PaymentResult> SendPaymentAsync(string toAccountNumber, string toBankName, decimal amount, string description, string apiKey, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var bankBaseUrl = _config["commercialBank"] ?? "http://localhost:8085";
        httpClient.BaseAddress = new Uri(bankBaseUrl);

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

        var requestBody = new
        {
            to_account_number = toAccountNumber,
            to_bank_name = toBankName,
            amount,
            description
        };

        var response = await RetryHelper.RetryAsync(
            () => httpClient.PostAsJsonAsync("/transaction", requestBody, cancellationToken),
            operationName: "Send payment");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApplicationException($"Payment failed: {response.StatusCode} - {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<PaymentResult>(cancellationToken: cancellationToken);
        return result!;
    }

    public class PaymentResult
    {
        public bool success { get; set; }
        public string transaction_number { get; set; } = default!;
        public string status { get; set; } = default!;
    }
}
