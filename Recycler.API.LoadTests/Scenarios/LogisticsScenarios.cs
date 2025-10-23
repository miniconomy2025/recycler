using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class LogisticsScenarios
    {
        public static ScenarioProps CreateProcessLogisticsScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("process_logistics", async context =>
            {
                try
                {
                    var logisticsData = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = context.Random.Next(0, 2) == 0 ? "PICKUP" : "DELIVERY",
                        Items = new[]
                        {
                            new { ItemId = Guid.NewGuid().ToString(), Quantity = context.Random.Next(1, 10), Type = "Phone" },
                            new { ItemId = Guid.NewGuid().ToString(), Quantity = context.Random.Next(1, 5), Type = "Battery" }
                        }
                    };

                    var json = JsonSerializer.Serialize(logisticsData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/logistics", content);
                    return Response.Ok(statusCode: response.StatusCode.ToString());
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message, "500", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 3, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(config.SteadyStateDurationSeconds))
            );
        }
        
        public static ScenarioProps CreateConsumerDeliveryScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("consumer_delivery", async context =>
                {
                    try
                    {
                        var consumerDeliveryData = new
                        {
                            Status = "DELIVERED",
                            ModelName = $"Phone_{context.Random.Next(1, 10)}",
                            Quantity = context.Random.Next(1, 5)
                        };

                        var json = JsonSerializer.Serialize(consumerDeliveryData, HttpClientFactory.GetJsonOptions());
                        var content = new StringContent(json, Encoding.UTF8);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                        var response = await httpClient.PostAsync("/logistics/consumer-deliveries", content);
                        return Response.Ok(statusCode: response.StatusCode.ToString());
                    }
                    catch (Exception ex)
                    {
                        return Response.Fail(ex.Message, "500", 0, 0);
                    }
                })
                .WithLoadSimulations(
                    Simulation.Inject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(config.SteadyStateDurationSeconds))
                );
        }
    }
}
