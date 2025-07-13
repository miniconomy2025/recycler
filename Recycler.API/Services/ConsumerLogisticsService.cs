using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Recycler.API.Models.ExternalApiRequests;

namespace Recycler.API.Services;

public class ConsumerLogisticsService(IHttpClientFactory clientFactory, IConfiguration config)
{
    private readonly HttpClient _client = clientFactory.CreateClient("test");
    private readonly IConfiguration _config = config;

    public async Task<HttpResponseMessage> SendDeliveryOrderAsync(DeliveryOrderRequestDto order)
    {
        var endpoint = new Uri($"{_config["consumerLogistic"]?.TrimEnd('/')}/api/pickups");

        try
        {
            var response = await _client.PostAsJsonAsync(endpoint, order);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }
}