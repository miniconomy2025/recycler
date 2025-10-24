using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class MachinePaymentScenarios
    {
        public static ScenarioProps CreateMachinePaymentConfirmationScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("machine_payment_confirmation", async context =>
            {
                try
                {
                    var machinePaymentData = new
                    {
                        OrderId = Guid.NewGuid().ToString(),
                        TotalWeight = context.Random.Next(50, 200)
                    };

                    var json = JsonSerializer.Serialize(machinePaymentData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/machine-payment-confirmation", content);
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


