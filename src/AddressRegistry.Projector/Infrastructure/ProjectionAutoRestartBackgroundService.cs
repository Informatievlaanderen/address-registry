namespace AddressRegistry.Projector.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Restarts projections that stopped unexpectedly (crash, timeout, ...) with an increasing backoff.
    /// Only projections whose persisted user desired state is explicitly "Started" are restarted;
    /// a projection that was stopped on request (desired state "Stopped") is never touched.
    /// </summary>
    public sealed class ProjectionAutoRestartBackgroundService : BackgroundService
    {
        private const string DesiredStateStarted = "Started";

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

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
        private readonly Dictionary<string, ProjectionRetryState> _retryStates = new();

        public ProjectionAutoRestartBackgroundService(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _logger = loggerFactory.CreateLogger<ProjectionAutoRestartBackgroundService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{Service} started.", nameof(ProjectionAutoRestartBackgroundService));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Delay first: gives Resume() at application start the chance to start the projections
                    // and guarantees we never spin in a tight loop when a check throws.
                    await Task.Delay(CheckInterval, stoppingToken);

                    try
                    {
                        await CheckAndRestartProjectionsAsync(stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Error while checking projections for auto-restart.");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
            }

            _logger.LogInformation("{Service} stopped.", nameof(ProjectionAutoRestartBackgroundService));
        }

        private async Task CheckAndRestartProjectionsAsync(CancellationToken cancellationToken)
        {
            var stoppedProjectionIds = new List<string>();
            foreach (var projection in _projectionsManager.GetRegisteredProjections())
            {
                var projectionId = projection.Id.ToString();
                if (projection.State == ConnectedProjectionState.Stopped)
                    stoppedProjectionIds.Add(projectionId);
                else
                    _retryStates.Remove(projectionId); // running again, reset backoff
            }

            if (stoppedProjectionIds.Count == 0)
                return;

            var desiredStates = await GetDesiredStates(cancellationToken);

            foreach (var projectionId in stoppedProjectionIds)
            {
                if (!IsDesiredStarted(desiredStates, projectionId))
                {
                    // Stopped on request (or never started): respect it and forget any backoff.
                    _retryStates.Remove(projectionId);
                    continue;
                }

                if (!_retryStates.TryGetValue(projectionId, out var retryState))
                {
                    retryState = new ProjectionRetryState();
                    _retryStates[projectionId] = retryState;
                }

                if (!retryState.ShouldRetry())
                    continue;

                // Re-read the desired state right before starting, so a stop request that came in
                // during this check is not overruled.
                if (!IsDesiredStarted(await GetDesiredStates(cancellationToken), projectionId)
                    || _projectionsManager.GetRegisteredProjections()
                        .SingleOrDefault(x => x.Id.ToString() == projectionId)?.State != ConnectedProjectionState.Stopped)
                {
                    continue;
                }

                retryState.RecordAttempt();

                _logger.LogWarning(
                    "Projection '{ProjectionId}' is stopped but desired state is '{DesiredState}'. Attempting restart (attempt {AttemptNumber}).",
                    projectionId,
                    DesiredStateStarted,
                    retryState.RetryCount);

                try
                {
                    await _projectionsManager.Start(projectionId, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(
                        ex,
                        "Failed to restart projection '{ProjectionId}' (attempt {AttemptNumber}).",
                        projectionId,
                        retryState.RetryCount);
                }
            }
        }

        private async Task<ILookup<string, string?>> GetDesiredStates(CancellationToken cancellationToken)
            => (await _projectionsManager.GetProjectionStates(cancellationToken))
                .ToLookup(x => x.Name, x => x.DesiredState, StringComparer.OrdinalIgnoreCase);

        private static bool IsDesiredStarted(ILookup<string, string?> desiredStates, string projectionId)
        {
            // Only restart when every known desired state for this projection is explicitly "Started".
            var states = desiredStates[projectionId].ToList();
            return states.Count > 0
                   && states.All(x => string.Equals(x, DesiredStateStarted, StringComparison.OrdinalIgnoreCase));
        }

        private sealed class ProjectionRetryState
        {
            private DateTimeOffset _lastAttempt = DateTimeOffset.MinValue;

            public int RetryCount { get; private set; }

            public bool ShouldRetry()
            {
                var backoff = BackoffIntervals[Math.Min(RetryCount, BackoffIntervals.Length - 1)];
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
