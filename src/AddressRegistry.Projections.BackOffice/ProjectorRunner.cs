namespace AddressRegistry.Projections.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class ProjectorRunner : BackgroundService
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly ILogger<ProjectorRunner> _logger;

        public ProjectorRunner(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _logger = loggerFactory.CreateLogger<ProjectorRunner>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Projector starting");
            await _projectionsManager.Start(stoppingToken);
            _logger.LogInformation("Projector started");
        }
    }
}
