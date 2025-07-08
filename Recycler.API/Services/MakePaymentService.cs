namespace Recycler.API.Services;

public class MakePaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MakePaymentService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClient = httpClientFactory.CreateClient();
        _config = config;

        var bankBaseUrl = _config["commercialBank"] ?? "http://localhost:8085";
        _httpClient.BaseAddress = new Uri(bankBaseUrl);
    }

    public async Task<PaymentResult> SendPaymentAsync(string toAccountNumber, string toBankName, decimal amount, string description, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            to_account_number = toAccountNumber,
            to_bank_name = toBankName,
            amount,
            description
        };

        var response = await _httpClient.PostAsJsonAsync("/transaction", requestBody, cancellationToken);
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
