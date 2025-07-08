namespace Recycler.API.Services;

public class ThohBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var thohService = scope.ServiceProvider.GetRequiredService<ThohService>();
                await thohService.GetAndUpdateRawMaterialPrice();
            }

            await Task.Delay(TimeSpan.FromMinutes(14), stoppingToken);
        }
    }

}