using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class MaterialsScenarios
    {
        public static ScenarioProps CreateGetMaterialsScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("get_materials", async context =>
            {
                try
                {
                    var response = await httpClient.GetAsync("/materials");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var responseTime = response.Headers.Date?.DateTime ?? DateTime.UtcNow;
                        
                        return Response.Ok(
                            statusCode: response.StatusCode.ToString(),
                            content.Length
                        );
                    }
                    else
                    {
                        return Response.Fail(
                            $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                            response.StatusCode.ToString(),
                            0
                        );
                    }
                }
                catch (HttpRequestException ex)
                {
                    return Response.Fail($"Network error: {ex.Message}", "NETWORK_ERROR", 0);
                }
                catch (TaskCanceledException ex)
                {
                    return Response.Fail($"Timeout: {ex.Message}", "TIMEOUT", 0);
                }
                catch (Exception ex)
                {
                    return Response.Fail($"Unexpected error: {ex.Message}", "UNKNOWN_ERROR", 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 4, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(config.SteadyStateDurationSeconds))
            );
        }
    }
}


