using Microsoft.Extensions.Configuration;
using Recycler.API.LoadTests.Configuration;

namespace Recycler.API.LoadTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Recycler API Load Tests");
            Console.WriteLine("======================");
            Console.WriteLine();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var testConfig = TestConfiguration.LoadFromConfiguration(configuration);

            Console.WriteLine($"Testing against: {testConfig.BaseUrl}");
            Console.WriteLine();

            var loadTests = new RecyclerApiLoadTests(testConfig);

            if (args.Length > 0 && args[0].ToLower() == "smoke")
            {
                Console.WriteLine("Running smoke test...");
                loadTests.RunQuickSmokeTest();
            }
            else
            {
                Console.WriteLine("Running comprehensive load test...");
                loadTests.RunLoadTests();
            }

            Console.WriteLine();
            Console.WriteLine("Load test completed. Check the reports folder for detailed results.");
        }
    }
}