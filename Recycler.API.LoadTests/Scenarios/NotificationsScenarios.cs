using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class NotificationsScenarios
    {
        public static ScenarioProps CreateNotifyMeScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("notify_me", async context =>
            {
                try
                {
                    var response = await httpClient.PostAsync("/recycler/notify-me", null);
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

        public static ScenarioProps CreateMachineFailureScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("machine_failure", async context =>
            {
                try
                {
                    var failureData = new
                    {
                        MachineId = Guid.NewGuid().ToString(),
                        FailureType = "Mechanical",
                        Description = "Test failure for load testing",
                        Severity = "Medium"
                    };

                    var json = JsonSerializer.Serialize(failureData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/machine-failure", content);
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
