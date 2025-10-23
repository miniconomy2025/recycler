using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;

namespace Recycler.API.LoadTests.Scenarios
{
    public static class ScenarioFactory
    {
        public static ScenarioProps[] CreateAllScenarios(TestConfiguration config)
        {
            var httpClient = HttpClientFactory.CreateHttpClient(config.BaseUrl);
            
            return new[]
            {
                MaterialsScenarios.CreateGetMaterialsScenario(httpClient, config),
                LogisticsScenarios.CreateProcessLogisticsScenario(httpClient, config),
                SimulationScenarios.CreateStartSimulationScenario(httpClient, config),
                OrdersScenarios.CreateGetOrderByIdScenario(httpClient, config),
                OrdersScenarios.CreateGetOrderByOrderNumberScenario(httpClient, config),
                OrdersScenarios.CreateCreateOrderScenario(httpClient, config),
                RevenueScenarios.CreateGetRevenueScenario(httpClient, config),
                NotificationsScenarios.CreateNotifyMeScenario(httpClient, config),
                NotificationsScenarios.CreateMachineFailureScenario(httpClient, config),
                BankNotificationScenarios.CreateBankNotificationScenario(httpClient, config),
                MachinePaymentScenarios.CreateMachinePaymentConfirmationScenario(httpClient, config)
            };
        }

        public static ScenarioProps[] CreateSmokeTestScenarios(TestConfiguration config)
        {
            var httpClient = HttpClientFactory.CreateHttpClient(config.BaseUrl);
            
            return new[]
            {
                MaterialsScenarios.CreateGetMaterialsScenario(httpClient, config)
                    .WithLoadSimulations(Simulation.Inject(rate: 1, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))),
                OrdersScenarios.CreateGetOrderByIdScenario(httpClient, config)
                    .WithLoadSimulations(Simulation.Inject(rate: 1, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)))
            };
        }

    }
}