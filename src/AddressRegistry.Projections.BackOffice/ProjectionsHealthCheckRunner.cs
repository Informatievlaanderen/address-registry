namespace AddressRegistry.Projections.BackOffice
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class ProjectionsHealthCheckRunner : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<ProjectionsHealthCheckRunner> _logger;

        private Timer? _timer;

        public ProjectionsHealthCheckRunner(
            IConnectedProjectionsManager projectionsManager,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory logger)
        {
            _projectionsManager = projectionsManager;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger.CreateLogger<ProjectionsHealthCheckRunner>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Projections health check starting");

            _timer = new Timer(RunHealthCheck, null, TimeSpan.Zero, Interval);

            _logger.LogInformation("Projections health check started");

            return Task.CompletedTask;
        }

        private void RunHealthCheck(object? state)
        {
            var registeredProjections =
                _projectionsManager.GetRegisteredProjections();

            if (registeredProjections
                .Any(x => x.State == ConnectedProjectionState.Stopped))
            {
                _logger.LogInformation("Projections with status Stopped detected. Shutting down service.");

                _hostApplicationLifetime.StopApplication();
            }
        }
    }
}
