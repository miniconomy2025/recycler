using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Services.Interfaces;

public class RecyclingBackgroundService : BackgroundService
    {
        private readonly ILogger<RecyclingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

        public RecyclingBackgroundService(
            ILogger<RecyclingBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recycling Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running recycling process at: {time}", DateTimeOffset.Now);

                    // Create a scope to get the recycling service
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var recyclingService = scope.ServiceProvider.GetRequiredService<IRecyclingService>();
                        
                        var result = await recyclingService.StartRecyclingAsync();

                        if (result.Success)
                        {
                            _logger.LogInformation(
                                "Recycling completed successfully. Processed {phonesProcessed} phones. Message: {message}", 
                                result.PhonesProcessed, 
                                result.Message);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Recycling process failed or partially completed. Message: {message}", 
                                result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while running the recycling process.");
                }

                // Wait for 2 minutes before the next run
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Recycling Background Service is stopping.");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recycling Background Service is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }