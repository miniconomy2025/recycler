using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class BankNotificationScenarios
    {
        public static ScenarioProps CreateBankNotificationScenario(HttpClient httpClient, TestConfiguration config)
        {
            return Scenario.Create("bank_notification", async context =>
            {
                try
                {
                    var bankNotificationData = new
                    {
                        transaction_number = Guid.NewGuid().ToString(),
                        status = "success",
                        description = context.Random.Next(1, 1000).ToString(),
                        amount = context.Random.Next(100, 5000),
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };

                    var json = JsonSerializer.Serialize(bankNotificationData, HttpClientFactory.GetJsonOptions());
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/api/banknotification", content);
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
