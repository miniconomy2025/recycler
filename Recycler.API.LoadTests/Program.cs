using Microsoft.Extensions.Configuration;
using Recycler.API.LoadTests.Configuration;

namespace Recycler.API.LoadTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Recycler API Load Tests & Performance Tests");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var testConfig = TestConfiguration.LoadFromConfiguration(configuration);
            var performanceConfig = PerformanceTestConfiguration.LoadFromConfiguration(configuration);

            Console.WriteLine($"Testing against: {testConfig.BaseUrl}");
            Console.WriteLine();

            var loadTests = new RecyclerApiLoadTests(testConfig);
            var performanceTests = new PerformanceTestRunner(performanceConfig);

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "smoke":
                        Console.WriteLine("Running smoke test...");
                        loadTests.RunQuickSmokeTest();
                        break;
                    case "performance":
                        Console.WriteLine("Running performance tests...");
                        performanceTests.RunPerformanceTests();
                        break;
                    case "stress":
                        Console.WriteLine("Running stress tests...");
                        performanceTests.RunStressTests();
                        break;
                    case "spike":
                        Console.WriteLine("Running spike tests...");
                        performanceTests.RunSpikeTests();
                        break;
                    case "all-performance":
                        Console.WriteLine("Running all performance tests...");
                        performanceTests.RunAllPerformanceTests();
                        break;
                    default:
                        Console.WriteLine("Running comprehensive load test...");
                        loadTests.RunLoadTests();
                        break;
                }
            }
            else
            {
                Console.WriteLine("Running comprehensive load test...");
                loadTests.RunLoadTests();
            }

            Console.WriteLine();
            Console.WriteLine("Test completed. Check the reports folder for detailed results.");
        }
    }
}


