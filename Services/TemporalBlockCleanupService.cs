using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace assignment.Services
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly BlockedCountriesService _blockedCountriesService;
        private readonly ILogger<TemporalBlockCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        public TemporalBlockCleanupService(
            BlockedCountriesService blockedCountriesService,
            ILogger<TemporalBlockCleanupService> logger)
        {
            _blockedCountriesService = blockedCountriesService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _blockedCountriesService.CleanupExpiredTemporalBlocks();
                    _logger.LogInformation("Temporal block cleanup completed at: {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up temporal blocks");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
} 