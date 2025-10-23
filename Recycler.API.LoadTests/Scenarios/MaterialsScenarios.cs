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
    }
}
