using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class StressScenarios
    {
        public static ScenarioProps[] CreateStressTestScenarios(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return new[]
            {
                CreateGradualLoadIncreaseScenario(httpClient, config),
                CreateSuddenLoadSpikeScenario(httpClient, config),
                CreateResourceExhaustionScenario(httpClient, config),
                CreateMemoryPressureScenario(httpClient, config)
            };
        }

        public static ScenarioProps CreateGradualLoadIncreaseScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("gradual_load_increase", async context =>
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
                    rate: 1,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.StressRampUpSeconds)
                ),
                Simulation.Inject(
                    rate: config.StressMaxUsers,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.StressTestDurationSeconds - config.StressRampUpSeconds)
                )
            );
        }

        public static ScenarioProps CreateSuddenLoadSpikeScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("sudden_load_spike", async context =>
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
                    during: TimeSpan.FromSeconds(30)
                ),
                Simulation.Inject(
                    rate: config.StressMaxUsers * 2,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(60)
                ),
                Simulation.Inject(
                    rate: 5,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(30)
                )
            );
        }

        public static ScenarioProps CreateResourceExhaustionScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("resource_exhaustion", async context =>
            {
                try
                {
                    var tasks = new List<Task<HttpResponseMessage>>();
                    
                    for (int i = 0; i < 10; i++)
                    {
                        var task = httpClient.GetAsync("/materials");
                        tasks.Add(task);
                    }
                    
                    var responses = await Task.WhenAll(tasks);
                    
                    var successCount = responses.Count(r => r.IsSuccessStatusCode);
                    var totalSize = 0;
                    
                    foreach (var response in responses)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            totalSize += content.Length;
                        }
                    }
                    
                    return Response.Ok(
                        statusCode: "200",
                        totalSize);
                }
                catch (Exception ex)
                {
                    return Response.Fail($"Error: {ex.Message}", "ERROR", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: config.StressMaxUsers / 2,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.StressTestDurationSeconds)
                )
            );
        }

        public static ScenarioProps CreateMemoryPressureScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("memory_pressure", async context =>
            {
                try
                {
                    var largeData = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "MEMORY_TEST",
                        Items = Enumerable.Range(1, 1000).Select(i => new
                        {
                            ItemId = Guid.NewGuid().ToString(),
                            Quantity = context.Random.Next(1, 100),
                            Type = $"TestItem_{i}",
                            Description = new string('A', 1000)
                        }).ToArray()
                    };
                    
                    var json = JsonSerializer.Serialize(largeData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await httpClient.PostAsync("/logistics", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return Response.Ok(
                            statusCode: response.StatusCode.ToString(),
                            responseContent.Length);
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
                    during: TimeSpan.FromSeconds(config.StressTestDurationSeconds)
                )
            );
        }
    }
}


