using System.Text;
using System.Text.Json;
using Recycler.API.Models.ExternalApiRequests;

namespace Recycler.API.Services;

public class ConsumerLogisticsService(IHttpClientFactory clientFactory, IConfiguration config)
{
    private readonly HttpClient _client = clientFactory.CreateClient(nameof(ConsumerLogisticsService));

    public async Task<HttpResponseMessage> SendDeliveryOrderAsync(DeliveryOrderRequestDto order)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(order),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _client.PostAsync("delivery-order", content);
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (Exception)
        {
            throw;
        }
    }
}