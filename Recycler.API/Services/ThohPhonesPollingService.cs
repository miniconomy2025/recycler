namespace Recycler.API.Services;

public class ThohPhonesPollingService : BackgroundService
{
    private readonly ILogger<ThohPhonesPollingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

    public ThohPhonesPollingService(ILogger<ThohPhonesPollingService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("THoH phones polling service started. Interval={Minutes} minute(s)", _interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var orchestrator = scope.ServiceProvider.GetRequiredService<AvailablePhonesNotificationService>();

                    _logger.LogInformation("Polling THoH for available phones...");

                    var result = await orchestrator.NotifyAsync(null, stoppingToken);

                    if (result.Success)
                    {
                        _logger.LogInformation("Notify flow completed: {Message}", result.Message);
                    }
                    else
                    {
                        _logger.LogInformation("No phones available to process");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while polling THoH phones");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        _logger.LogInformation("THoH phones polling service stopped");
    }
}


