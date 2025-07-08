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

        builder.Services.AddHttpClient<ThohService>(client =>
        {
            client.BaseAddress = new Uri(thoHUrl);
        });

        builder.Services.AddScoped<ThohService>();
        builder.Services.AddHostedService<ThohBackgroundService>();
    }
}