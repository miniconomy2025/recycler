using Recycler.API.Utils;

namespace Recycler.API.Services;

public class BankAccountService
{
    private readonly HttpClient _http;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(IHttpClientFactory factory, IConfiguration config, ILogger<BankAccountService> logger)
    {
        _http = factory.CreateClient("test");
        var bankUrl = config["commercialBankUrl"] ?? "http://localhost:8085";
        _http.BaseAddress = new Uri(bankUrl);
        _logger = logger;
    }

    public async Task<string?> RegisterAsync(string notificationUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to register bank account with notification URL: {NotificationUrl}", notificationUrl);
        var response = await RetryHelper.RetryAsync(
            () => _http.PostAsJsonAsync("api/account", new { notification_url = notificationUrl }, cancellationToken),
            operationName: "Create bank account");

        _logger.LogInformation("Bank account creation response status: {StatusCode}", response.StatusCode);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogInformation("Account already exists, fetching existing account details");
            response = await RetryHelper.RetryAsync(
            () => _http.GetAsync("api/account/me", cancellationToken),
            operationName: "Check bank account");
        }

        var data = await response.Content.ReadFromJsonAsync<AccountResponse>(cancellationToken: cancellationToken);
        var accountNumber = data?.account_number;
        
        if (accountNumber != null)
        {
            _logger.LogInformation("Bank account registration successful: {AccountNumber}", accountNumber);
        }
        else
        {
            _logger.LogError("Bank account registration failed - no account number returned");
        }
        
        return accountNumber;
    }

    private class AccountResponse
    {
        public string account_number { get; set; } = default!;
    }
}
