namespace AddressRegistry.Projector.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class ProjectionAutoRestartBackgroundService : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan MaxBackoff = TimeSpan.FromHours(1);
        private const int MaxRetryCount = 5;

        private static readonly TimeSpan[] BackoffIntervals =
        [
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromHours(1)
        ];

        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly ILogger<ProjectionAutoRestartBackgroundService> _logger;
        private readonly ConcurrentDictionary<string, ProjectionRetryState> _retryStates = new();

        public ProjectionAutoRestartBackgroundService(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _logger = loggerFactory.CreateLogger<ProjectionAutoRestartBackgroundService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProjectionAutoRestartBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(CheckInterval, stoppingToken);
                    await CheckAndRestartProjectionsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking projections for auto-restart.");
                }
            }

            _logger.LogInformation("ProjectionAutoRestartBackgroundService stopped.");
        }

        private async Task CheckAndRestartProjectionsAsync(CancellationToken cancellationToken)
        {
            var registeredProjections = _projectionsManager.GetRegisteredProjections().ToList();
            var projectionStates = (await _projectionsManager.GetProjectionStates(cancellationToken)).ToList();

            foreach (var projection in registeredProjections)
            {
                var projectionId = projection.Id.ToString();

                if (projection.State != ConnectedProjectionState.Stopped)
                {
                    // Projection is running, reset retry state
                    _retryStates.TryRemove(projectionId, out _);
                    continue;
                }

                var projectionState = projectionStates.SingleOrDefault(x => x.Name == projectionId);
                if (projectionState?.DesiredState != "Started")
                {
                    continue;
                }

                // Projection is stopped but desired state is "Started" - try to restart
                var retryState = _retryStates.GetOrAdd(projectionId, _ => new ProjectionRetryState());

                if (!retryState.ShouldRetry())
                {
                    continue;
                }

                _logger.LogWarning(
                    "Projection '{ProjectionId}' is stopped but desired state is 'Started'. Attempting restart (attempt {AttemptNumber}).",
                    projectionId,
                    retryState.RetryCount + 1);

                try
                {
                    await _projectionsManager.Start(projectionId, cancellationToken);
                    retryState.RecordAttempt();

                    _logger.LogInformation(
                        "Restart command sent for projection '{ProjectionId}'.",
                        projectionId);
                }
                catch (Exception ex)
                {
                    retryState.RecordAttempt();
                    _logger.LogError(
                        ex,
                        "Failed to restart projection '{ProjectionId}' (attempt {AttemptNumber}).",
                        projectionId,
                        retryState.RetryCount);
                }
            }
        }

        private sealed class ProjectionRetryState
        {
            public int RetryCount { get; private set; }
            private DateTimeOffset _lastAttempt = DateTimeOffset.MinValue;

            public bool ShouldRetry()
            {
                var backoff = RetryCount < MaxRetryCount
                    ? BackoffIntervals[RetryCount]
                    : MaxBackoff;

                return DateTimeOffset.UtcNow - _lastAttempt >= backoff;
            }

            public void RecordAttempt()
            {
                RetryCount++;
                _lastAttempt = DateTimeOffset.UtcNow;
            }
        }
    }
}
