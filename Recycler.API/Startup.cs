using Recycler.API.Services;

namespace Recycler.API;

public class Startup(WebApplicationBuilder builder)
{
    public async Task ConfigureApplication()
    {
        await SetupExternalApiClients();
    }

    private async Task SetupExternalApiClients()
    {
        builder.Services.AddHttpClient<ThohService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:8084");
        });

        builder.Services.AddScoped<ThohService>();
        builder.Services.AddHostedService<ThohBackgroundService>();
    }
}