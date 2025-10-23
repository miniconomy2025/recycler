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
        var thoHUrl = string.IsNullOrEmpty(builder.Configuration["thoHApiUrl"]) ? "http://localhost:8084" : builder.Configuration["thoHApiUrl"];

        builder.Services.AddHttpClient<ThohService>(client =>
        {
            client.BaseAddress = new Uri(thoHUrl);
        });

        builder.Services.AddScoped<ThohService>();

        builder.Services.AddHostedService<ThohBackgroundService>();
        builder.Services.AddHostedService<ThohPhonesPollingService>();

        var consumerLogisticsUrl = builder.Configuration["consumerLogistic"] ?? "http://localhost:8086";
        var bankUrl = builder.Configuration["commercialBankUrl"] ?? "http://localhost:8085";

        builder.Services.AddHttpClient<ConsumerLogisticsService>(client =>
        {
            client.BaseAddress = new Uri(consumerLogisticsUrl);
        });

        builder.Services.AddHttpClient<ThohService>(client =>
        {
            client.BaseAddress = new Uri(consumerLogisticsUrl);
        });

        builder.Services.AddHttpClient<CommercialBankService>(client =>
        {
            client.BaseAddress = new Uri(bankUrl);
        });

        builder.Services.AddScoped<CommercialBankService>();
        builder.Services.AddScoped<ThohService>();
        builder.Services.AddScoped<ConsumerLogisticsService>();
        builder.Services.AddScoped<AvailablePhonesNotificationService>();
    }
}