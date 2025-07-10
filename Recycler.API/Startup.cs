using Recycler.API.Services;

namespace Recycler.API;

public class Startup(WebApplicationBuilder builder)
{
    public void ConfigureApplication()
    {
        SetupExternalApiClients();
    }

    private void SetupExternalApiClients()
    {
        var thoHUrl = builder.Configuration["thoHApiUrl"] ?? "http://localhost:8084";
        var consumerLogisticsUrl = builder.Configuration["consumerLogistic"] ?? "http://localhost:8086";

        builder.Services.AddHttpClient<ConsumerLogisticsService>(client =>
        {
            client.BaseAddress = new Uri(consumerLogisticsUrl);
        });

        builder.Services.AddScoped<ConsumerLogisticsService>();
        builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
        builder.Services.AddHostedService<ThohBackgroundService>();
    }
}