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
        var thoHUrl =string.IsNullOrEmpty(builder.Configuration["thoHApiUrl"]) ? "http://localhost:8084" : builder.Configuration["thoHApiUrl"];
        
        builder.Services.AddHttpClient<ThohService>(client =>
        {
            client.BaseAddress = new Uri(thoHUrl);
        });

        builder.Services.AddScoped<ThohService>();
        
        builder.Services.AddHostedService<ThohBackgroundService>();
        
        var consumerLogisticsUrl = builder.Configuration["consumerLogistic"] ?? "http://localhost:8086";

        builder.Services.AddHttpClient<ConsumerLogisticsService>(client =>
        {
            client.BaseAddress = new Uri(consumerLogisticsUrl);
        });

        builder.Services.AddScoped<ConsumerLogisticsService>();
    }
}