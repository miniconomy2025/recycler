using System.Text.Json;

namespace Recycler.API.LoadTests.Configuration
{
    public static class HttpClientFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static HttpClient CreateHttpClient(string baseUrl)
        {
            var handler = new HttpClientHandler();
            
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "Recycler.API.LoadTests/1.0");

            return client;
        }

        public static JsonSerializerOptions GetJsonOptions() => JsonOptions;
    }
}
