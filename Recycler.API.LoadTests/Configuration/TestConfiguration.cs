using Microsoft.Extensions.Configuration;

namespace Recycler.API.LoadTests.Configuration
{
    public class TestConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int RampUpDurationSeconds { get; set; } = 30;
        public int SteadyStateDurationSeconds { get; set; } = 60;
        public int RampDownDurationSeconds { get; set; } = 30;
        public int MinUsers { get; set; } = 1;
        public int MaxUsers { get; set; } = 10;
        public int RequestTimeoutSeconds { get; set; } = 30;

        public static TestConfiguration LoadFromConfiguration(IConfiguration configuration)
        {
            return new TestConfiguration
            {
                BaseUrl = configuration["recyclerApi:baseUrl"] ?? "https://localhost:7001",
                RampUpDurationSeconds = int.Parse(configuration["LoadTest:RampUpDurationSeconds"] ?? "30"),
                SteadyStateDurationSeconds = int.Parse(configuration["LoadTest:SteadyStateDurationSeconds"] ?? "60"),
                RampDownDurationSeconds = int.Parse(configuration["LoadTest:RampDownDurationSeconds"] ?? "30"),
                MinUsers = int.Parse(configuration["LoadTest:MinUsers"] ?? "1"),
                MaxUsers = int.Parse(configuration["LoadTest:MaxUsers"] ?? "10"),
                RequestTimeoutSeconds = int.Parse(configuration["LoadTest:RequestTimeoutSeconds"] ?? "30")
            };
        }
    }
}
