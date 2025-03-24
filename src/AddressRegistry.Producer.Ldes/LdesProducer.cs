    namespace AddressRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class LdesProducer : BackgroundService
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<LdesProducer> _logger;

        public LdesProducer(
            IConnectedProjectionsManager projectionsManager,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = loggerFactory.CreateLogger<LdesProducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting ldes projections");
                await _projectionsManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"An error occurred while starting the {nameof(LdesProducer)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
