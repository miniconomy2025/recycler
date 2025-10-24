using NBomber;
using NBomber.CSharp;
using Recycler.API.LoadTests.Configuration;
using Recycler.API.LoadTests.Scenarios;

namespace Recycler.API.LoadTests
{
    public class PerformanceTestRunner
    {
        private readonly PerformanceTestConfiguration _config;

        public PerformanceTestRunner(PerformanceTestConfiguration config)
        {
            _config = config;
        }

        public void RunPerformanceTests()
        {
            Console.WriteLine("=== PERFORMANCE TESTS ===");
            Console.WriteLine($"Testing against: {_config.BaseUrl}");
            Console.WriteLine($"Test duration: {_config.PerformanceTestDurationSeconds} seconds");
            Console.WriteLine($"Max response time threshold: {_config.MaxResponseTimeMs}ms");
            Console.WriteLine($"Max error rate threshold: {_config.MaxErrorRatePercent}%");
            Console.WriteLine($"Min throughput threshold: {_config.MinThroughputRps} RPS");
            Console.WriteLine();

            var httpClient = HttpClientFactory.CreateHttpClient(_config.BaseUrl);
            var scenarios = PerformanceScenarios.CreatePerformanceTestScenarios(httpClient, _config);

            NBomberRunner
                .RegisterScenarios(scenarios)
                .WithReportFolder("performance_test_reports")
                .Run();
        }

        public void RunStressTests()
        {
            Console.WriteLine("=== STRESS TESTS ===");
            Console.WriteLine($"Testing against: {_config.BaseUrl}");
            Console.WriteLine($"Test duration: {_config.StressTestDurationSeconds} seconds");
            Console.WriteLine($"Max users: {_config.StressMaxUsers}");
            Console.WriteLine($"Ramp-up duration: {_config.StressRampUpSeconds} seconds");
            Console.WriteLine();

            var httpClient = HttpClientFactory.CreateHttpClient(_config.BaseUrl);
            var scenarios = StressScenarios.CreateStressTestScenarios(httpClient, _config);

            NBomberRunner
                .RegisterScenarios(scenarios)
                .WithReportFolder("stress_test_reports")
                .Run();
        }

        public void RunSpikeTests()
        {
            Console.WriteLine("=== SPIKE TESTS ===");
            Console.WriteLine($"Testing against: {_config.BaseUrl}");
            Console.WriteLine($"Test duration: {_config.SpikeTestDurationSeconds} seconds");
            Console.WriteLine($"Max users: {_config.SpikeMaxUsers}");
            Console.WriteLine($"Spike duration: {_config.SpikeDurationSeconds} seconds");
            Console.WriteLine();

            var httpClient = HttpClientFactory.CreateHttpClient(_config.BaseUrl);
            var scenarios = SpikeScenarios.CreateSpikeTestScenarios(httpClient, _config);

            NBomberRunner
                .RegisterScenarios(scenarios)
                .WithReportFolder("spike_test_reports")
                .Run();
        }

        public void RunAllPerformanceTests()
        {
            Console.WriteLine("=== COMPREHENSIVE PERFORMANCE TESTING ===");
            Console.WriteLine($"Testing against: {_config.BaseUrl}");
            Console.WriteLine();

            try
            {
                Console.WriteLine("1. Running Performance Tests...");
                RunPerformanceTests();
                Console.WriteLine("Performance tests completed.");
                Console.WriteLine();

                Console.WriteLine("2. Running Stress Tests...");
                RunStressTests();
                Console.WriteLine("Stress tests completed.");
                Console.WriteLine();

                Console.WriteLine("3. Running Spike Tests...");
                RunSpikeTests();
                Console.WriteLine("Spike tests completed.");
                Console.WriteLine();

                Console.WriteLine("=== ALL PERFORMANCE TESTS COMPLETED ===");
                Console.WriteLine("Check the following report folders:");
                Console.WriteLine("- performance_test_reports");
                Console.WriteLine("- stress_test_reports");
                Console.WriteLine("- spike_test_reports");
                Console.WriteLine("- volume_test_reports");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during performance testing: {ex.Message}");
                throw;
            }
        }
    }
}


