namespace AddressRegistry.Projections.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class HealthCheckRunner : BackgroundService
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<HealthCheckRunner> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5); // Check every 5 minutes

        public HealthCheckRunner(
            HealthCheckService healthCheckService,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<HealthCheckRunner> logger)
        {
            _healthCheckService = healthCheckService;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                var report = await _healthCheckService.CheckHealthAsync(stoppingToken);

                // Log the health check result
                if (report.Status == HealthStatus.Healthy)
                {
                    _logger.LogInformation("Database health check passed.");
                }
                else
                {
                    _logger.LogError("Database health check failed. Stopping application.");
                    foreach (var entry in report.Entries)
                    {
                        _logger.LogError($"{entry.Key}: {entry.Value.Status} - {entry.Value.Description}");
                    }

                    _hostApplicationLifetime.StopApplication();
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
