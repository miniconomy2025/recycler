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
        _client.BaseAddress = new Uri(_config["consumerLogistic"] ?? "");
        // var content = new StringContent(
        //     order,
        //     Encoding.UTF8,
        //     mediaType: "application/json"
        // );

        try
        {
            var response = await _client.PostAsJsonAsync("api/pickups", order);
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