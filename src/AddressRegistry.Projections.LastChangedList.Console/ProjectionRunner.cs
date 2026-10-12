namespace AddressRegistry.Projections.LastChangedList.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class ProjectionRunner : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

        // A projection has to be seen stopped on consecutive checks before it is considered failed,
        // so the time between issuing the start and the projection actually running is not seen as a failure.
        private const int StoppedChecksBeforeFailure = 2;

        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<ProjectionRunner> _logger;
        private readonly IConnectedProjectionsManager _projectionManager;

        public ProjectionRunner(
            IHostApplicationLifetime hostApplicationLifetime,
            IConnectedProjectionsManager projectionManager,
            ILogger<ProjectionRunner> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _projectionManager = projectionManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _projectionManager.Start(stoppingToken);
                await MonitorProjections(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(ProjectionRunner)}.");
                Environment.ExitCode = 1;
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task MonitorProjections(CancellationToken stoppingToken)
        {
            var stoppedChecks = new Dictionary<string, int>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(CheckInterval, stoppingToken);

                foreach (var projection in _projectionManager.GetRegisteredProjections())
                {
                    var projectionId = projection.Id.ToString();

                    if (projection.State != ConnectedProjectionState.Stopped)
                    {
                        stoppedChecks.Remove(projectionId);
                        continue;
                    }

                    stoppedChecks[projectionId] = stoppedChecks.GetValueOrDefault(projectionId) + 1;
                }

                var failedProjections = stoppedChecks
                    .Where(x => x.Value >= StoppedChecksBeforeFailure)
                    .Select(x => x.Key)
                    .ToList();

                if (failedProjections.Any())
                {
                    throw new InvalidOperationException(
                        $"Projection(s) {string.Join(", ", failedProjections)} stopped unexpectedly, quitting application.");
                }
            }
        }
    }
}
