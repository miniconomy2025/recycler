using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class SimulationScenarios
    {
        public static ScenarioProps CreateStartSimulationScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("start_simulation", async context =>
            {
                try
                {
                    var simulationData = new
                    {
                        SimulationId = Guid.NewGuid().ToString(),
                        Duration = context.Random.Next(60, 300),
                        Parameters = new
                        {
                            RecyclingRate = context.Random.NextDouble() * 0.5 + 0.5,
                            MachineEfficiency = context.Random.NextDouble() * 0.3 + 0.7,
                            MaterialQuality = context.Random.NextDouble() * 0.4 + 0.6
                        }
                    };

                    var json = JsonSerializer.Serialize(simulationData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/simulation", content);
                    return Response.Ok(statusCode: response.StatusCode.ToString());
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message, "500", 0, 0);
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 1, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(config.SteadyStateDurationSeconds))
            );
        }
    }
}


