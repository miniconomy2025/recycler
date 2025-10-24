using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class OrdersScenarios
    {
        public static ScenarioProps CreateGetOrderByIdScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("get_order_by_id", async context =>
            {
                try
                {
                    var orderId = context.Random.Next(1, 101);
                    
                    var response = await httpClient.GetAsync($"/orders?id={orderId}");
                    return Response.Ok(statusCode: response.StatusCode.ToString());
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message, "500", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 6, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(config.SteadyStateDurationSeconds))
            );
        }

        public static ScenarioProps CreateGetOrderByOrderNumberScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("get_order_by_order_number", async context =>
            {
                try
                {
                    var orderNumber = Guid.NewGuid();
                    
                    var response = await httpClient.GetAsync($"/orders/{orderNumber}");
                    return Response.Ok(statusCode: response.StatusCode.ToString());
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message, "500", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 4, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(config.SteadyStateDurationSeconds))
            );
        }

        public static ScenarioProps CreateCreateOrderScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("create_order", async context =>
            {
                try
                {
                    var createOrderData = new
                    {
                        CustomerName = $"TestCustomer_{Guid.NewGuid().ToString("N")[..8]}",
                        CustomerEmail = $"test{Guid.NewGuid().ToString("N")[..8]}@example.com",
                        Items = new[]
                        {
                            new { ItemName = "Copper", Quantity = context.Random.Next(1, 5) * 1000, Price = context.Random.Next(1000, 5000) },
                            new { ItemName = "Sand", Quantity = context.Random.Next(1, 5) * 1000, Price = context.Random.Next(100, 500) }
                        },
                        TotalAmount = context.Random.Next(2000, 10000)
                    };

                    var json = JsonSerializer.Serialize(createOrderData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/orders", content);
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


