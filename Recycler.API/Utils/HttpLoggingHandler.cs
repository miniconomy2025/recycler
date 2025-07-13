namespace Recycler.API.Utils;

public class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;

    public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log request
        _logger.LogInformation("➡️ HTTP {Method} {Url}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("📤 Request Body: {Body}", requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Log response
        _logger.LogInformation("⬅️ HTTP {StatusCode} {Url}", response.StatusCode, response.RequestMessage?.RequestUri);

        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("📥 Response Body: {Body}", responseBody);
        }

        return response;
    }
}
