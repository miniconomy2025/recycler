using Recycler.API.Utils;

namespace Recycler.API.Services;

public class BankAccountService
{
    private readonly HttpClient _http;

    public BankAccountService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("test");
        var bankUrl = config["commercialBankUrl"] ?? "http://localhost:8085";
        _http.BaseAddress = new Uri(bankUrl);
    }

    public async Task<string?> RegisterAsync(string notificationUrl, CancellationToken cancellationToken)
    {
        var response = await RetryHelper.RetryAsync(
            () => _http.PostAsJsonAsync("/api/account", new { notification_url = notificationUrl }, cancellationToken),
            operationName: "Create bank account");

        var data = await response.Content.ReadFromJsonAsync<AccountResponse>(cancellationToken: cancellationToken);
        return data?.account_number;
    }

    private class AccountResponse
    {
        public string account_number { get; set; } = default!;
    }
}
