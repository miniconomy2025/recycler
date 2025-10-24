using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class PerformanceScenarios
    {
        public static ScenarioProps[] CreatePerformanceTestScenarios(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return new[]
            {
                CreateHighThroughputScenario(httpClient, config),
                CreateLowLatencyScenario(httpClient, config),
                CreateMixedWorkloadScenario(httpClient, config),
                CreateConcurrentUsersScenario(httpClient, config)
            };
        }

        public static ScenarioProps CreateHighThroughputScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("high_throughput_test", async context =>
            {
                try
                {
                    var response = await httpClient.GetAsync("/materials");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Response.Ok(
                            statusCode: response.StatusCode.ToString(),
                            content.Length);
                    }
                    else
                    {
                        return Response.Fail(
                            $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                            response.StatusCode.ToString(),
                            0);
                    }
                }
                catch (Exception ex)
                {
                    return Response.Fail($"Error: {ex.Message}", "ERROR", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: 20,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.PerformanceTestDurationSeconds)
                )
            );
        }

        public static ScenarioProps CreateLowLatencyScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("low_latency_test", async context =>
            {
                try
                {
                    var response = await httpClient.GetAsync("/materials");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Response.Ok(
                            statusCode: response.StatusCode.ToString(),
                            content.Length);
                    }
                    else
                    {
                        return Response.Fail(
                            $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                            response.StatusCode.ToString(),
                            0);
                    }
                }
                catch (Exception ex)
                {
                    return Response.Fail($"Error: {ex.Message}", "ERROR", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: 5,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.PerformanceTestDurationSeconds)
                )
            );
        }

        public static ScenarioProps CreateMixedWorkloadScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("mixed_workload_test", async context =>
            {
                try
                {
                    var operation = context.Random.Next(0, 4);
                    
                    HttpResponseMessage response;
                    string content = "";
                    
                    switch (operation)
                    {
                        case 0:
                            response = await httpClient.GetAsync("/materials");
                            break;
                        case 1:
                            var orderId = context.Random.Next(1, 101);
                            response = await httpClient.GetAsync($"/orders?id={orderId}");
                            break;
                        case 2:
                            var logisticsData = new
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = context.Random.Next(0, 2) == 0 ? "PICKUP" : "DELIVERY",
                                Items = new[]
                                {
                                    new { ItemId = Guid.NewGuid().ToString(), Quantity = context.Random.Next(1, 10), Type = "Phone" }
                                }
                            };
                            
                            var json = JsonSerializer.Serialize(logisticsData, HttpClientFactory.GetJsonOptions());
                            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                            response = await httpClient.PostAsync("/logistics", jsonContent);
                            break;
                        default:
                            response = await httpClient.GetAsync("/internal/revenue/company-orders");
                            break;
                    }
                    
                    if (response.IsSuccessStatusCode)
                    {
                        content = await response.Content.ReadAsStringAsync();
                        return Response.Ok(
                            statusCode: response.StatusCode.ToString(),
                            content.Length);
                    }
                    else
                    {
                        return Response.Fail(
                            $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                            response.StatusCode.ToString(),
                            0);
                    }
                }
                catch (Exception ex)
                {
                    return Response.Fail($"Error: {ex.Message}", "ERROR", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: 10,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.PerformanceTestDurationSeconds)
                )
            );
        }

        public static ScenarioProps CreateConcurrentUsersScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("concurrent_users_test", async context =>
            {
                try
                {
                    var tasks = new List<Task<HttpResponseMessage>>();
                    
                    for (int i = 0; i < 3; i++)
                    {
                        var task = httpClient.GetAsync("/materials");
                        tasks.Add(task);
                    }
                    
                    var responses = await Task.WhenAll(tasks);
                    
                    var allSuccessful = responses.All(r => r.IsSuccessStatusCode);
                    
                    if (allSuccessful)
                    {
                        var totalSize = 0;
                        foreach (var response in responses)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            totalSize += content.Length;
                        }
                        
                        return Response.Ok(
                            statusCode: "200",
                            totalSize);
                    }
                    else
                    {
                        var failedCount = responses.Count(r => !r.IsSuccessStatusCode);
                        return Response.Fail(
                            $"{failedCount} out of {responses.Length} requests failed",
                            "PARTIAL_FAILURE",
                            0);
                    }
                }
                catch (Exception ex)
                {
                    return Response.Fail($"Error: {ex.Message}", "ERROR", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: 3,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.PerformanceTestDurationSeconds)
                )
            );
        }
    }
}


