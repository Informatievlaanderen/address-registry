namespace AddressRegistry.Projections.Elastic.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class ProjectionRunner : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ElasticIndex _elasticIndex;
        private readonly ILogger<ProjectionRunner> _logger;
        private readonly IConnectedProjectionsManager _projectionManager;

        public ProjectionRunner(
            IHostApplicationLifetime hostApplicationLifetime,
            ElasticIndex elasticIndex,
            IConnectedProjectionsManager projectionManager,
            ILogger<ProjectionRunner> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _elasticIndex = elasticIndex;
            _projectionManager = projectionManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //TODO-rik create index+update alias
                await _elasticIndex.CreateIndexIfNotExist(stoppingToken).ConfigureAwait(false);
                await _elasticIndex.EnsureAliasExistsAndPointsToIndex(stoppingToken).ConfigureAwait(false);

                await _projectionManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(ProjectionRunner)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
