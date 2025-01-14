namespace AddressRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class SnapshotProducer : BackgroundService
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<SnapshotProducer> _logger;

        public SnapshotProducer(
            IConnectedProjectionsManager projectionsManager,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = loggerFactory.CreateLogger<SnapshotProducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting snapshot projections");
                await _projectionsManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"An error occurred while starting the {nameof(SnapshotProducer)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
