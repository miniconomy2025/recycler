using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class SpikeScenarios
    {
        public static ScenarioProps[] CreateSpikeTestScenarios(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return new[]
            {
                CreateTrafficSpikeScenario(httpClient, config),
                CreateRecoveryTestScenario(httpClient, config),
                CreateMultipleSpikesScenario(httpClient, config),
                CreateSustainedSpikeScenario(httpClient, config)
            };
        }

        public static ScenarioProps CreateTrafficSpikeScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("traffic_spike", async context =>
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
                    rate: config.SpikeMaxUsers,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.SpikeDurationSeconds)
                ),
                Simulation.Inject(
                    rate: 5,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(30)
                )
            );
        }

        public static ScenarioProps CreateRecoveryTestScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("recovery_test", async context =>
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
                    rate: 10,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(20)
                ),
                Simulation.Inject(
                    rate: config.SpikeMaxUsers * 3,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(10)
                ),
                Simulation.Inject(
                    rate: 10,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(30)
                )
            );
        }

        public static ScenarioProps CreateMultipleSpikesScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("multiple_spikes", async context =>
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
                    during: TimeSpan.FromSeconds(20)
                ),
                Simulation.Inject(
                    rate: config.SpikeMaxUsers,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(15)
                ),
                Simulation.Inject(
                    rate: 5,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(20)
                ),
                Simulation.Inject(
                    rate: config.SpikeMaxUsers,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(15)
                ),
                Simulation.Inject(
                    rate: 5,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(20)
                )
            );
        }

        public static ScenarioProps CreateSustainedSpikeScenario(HttpClient httpClient, PerformanceTestConfiguration config)
        {
            return Scenario.Create("sustained_spike", async context =>
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
                    rate: config.SpikeMaxUsers,
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
    }
}


