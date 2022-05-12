namespace AddressRegistry.Consumer.Infrastructure
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Logging;

    public class ProjectorRunner
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly ILogger _logger;

        public ProjectorRunner(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _logger = loggerFactory.CreateLogger<ProjectorRunner>();
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Projector starting");
            await _projectionsManager.Start(cancellationToken);
            _logger.LogInformation("Projector started");

            await Task.Delay(10000, cancellationToken); //waiting for projections to get started

            while (!cancellationToken.IsCancellationRequested
                   && _projectionsManager
                       .GetRegisteredProjections()
                       .All(x => x.State != ConnectedProjectionState.Stopped))
            {
                await Task.Delay(1000, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Projector cancelled");
            }
            else
            {
                _logger.LogCritical("Projections went in a 'stopped' stated");
            }
        }
    }
}
