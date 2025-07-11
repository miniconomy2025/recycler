namespace Recycler.API.Services;

public class ThohBackgroundService : BackgroundService
{
    private IServiceScopeFactory _scopeFactory;
    private ILogService _logService;

    public ThohBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        using (var scope = scopeFactory.CreateScope())
        {
            _logService = scope.ServiceProvider.GetRequiredService<ILogService>();
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _logService.CreateLog(null, "Background Service: ThohBackgroundService",
            "THOH Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var thohService = scope.ServiceProvider.GetRequiredService<ThohService>();

                await _logService.CreateLog(null, "Background Service: ThohBackgroundService",
                    "Retrieving and updating the raw material prices.");

                await thohService.GetAndUpdateRawMaterialPrice();
            }

            await Task.Delay(TimeSpan.FromMinutes(14), stoppingToken);
        }

        await _logService.CreateLog(null, "Background Service: ThohBackgroundService",
            "THOH Background Service has stopped.");
    }

}