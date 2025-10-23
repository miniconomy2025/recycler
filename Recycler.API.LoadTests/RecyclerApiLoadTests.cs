using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using Recycler.API.LoadTests.Scenarios;

namespace Recycler.API.LoadTests
{
    public class RecyclerApiLoadTests
    {
        private readonly TestConfiguration _config;

        public RecyclerApiLoadTests(TestConfiguration config)
        {
            _config = config;
        }

        public void RunLoadTests()
        {
            Console.WriteLine($"Starting comprehensive load tests against: {_config.BaseUrl}");
            Console.WriteLine($"Test duration: {_config.SteadyStateDurationSeconds} seconds");
            Console.WriteLine($"Max users: {_config.MaxUsers}");
            Console.WriteLine();

            var scenarios = ScenarioFactory.CreateAllScenarios(_config);

            Console.WriteLine("Testing all API endpoints:");
            Console.WriteLine("- GET /materials");
            Console.WriteLine("- POST /logistics");
            Console.WriteLine("- POST /simulation");
            Console.WriteLine("- GET /orders?id={id}");
            Console.WriteLine("- GET /orders/{orderNumber}");
            Console.WriteLine("- POST /orders");
            Console.WriteLine("- GET /internal/revenue/company-orders");
            Console.WriteLine("- POST /recycler/notify-me");
            Console.WriteLine("- POST /machine-failure");
            Console.WriteLine("- POST /api/banknotification");
            Console.WriteLine("- POST /machine-payment-confirmation");
            Console.WriteLine();

            NBomberRunner
                .RegisterScenarios(scenarios)
                .WithReportFolder("load_test_reports")
                .Run();
        }

        public void RunQuickSmokeTest()
        {
            Console.WriteLine($"Running quick smoke test against: {_config.BaseUrl}");
            Console.WriteLine();

            var scenarios = ScenarioFactory.CreateSmokeTestScenarios(_config);

            NBomberRunner
                .RegisterScenarios(scenarios)
                .WithReportFolder("smoke_test_reports")
                .Run();
        }
    }
}
