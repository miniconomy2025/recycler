using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class RevenueScenarios
    {
        public static ScenarioProps CreateGetRevenueScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("get_revenue", async context =>
            {
                try
                {
                    var response = await httpClient.GetAsync("/internal/revenue/company-orders");
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
