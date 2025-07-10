using Recycler.API.Services;
using Recycler.API.Utils;

public class MakePaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly CommercialBankService _commercialBankService;
    private readonly ISimulationClock _simulationClock;

    public MakePaymentService(IHttpClientFactory httpClientFactory, IConfiguration config, CommercialBankService commercialBankService, ISimulationClock simulationClock)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _commercialBankService = commercialBankService;
        _simulationClock = simulationClock;
    }

    public async Task<PaymentResult> SendPaymentAsync(string toAccountNumber, string toBankName, decimal amount, string description, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var bankBaseUrl = _config["commercialBank"] ?? "";
        httpClient.BaseAddress = new Uri(bankBaseUrl);

        httpClient.DefaultRequestHeaders.Clear();
        var simTime = _simulationClock.GetCurrentSimulationTime();

        var requestBody = new PaymentRequestDto
        {
            amount = amount,
            description = description,
            from = _commercialBankService.AccountNumber,
            to = toAccountNumber,
            timestamp = ((DateTimeOffset)simTime).ToUnixTimeSeconds(),
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
