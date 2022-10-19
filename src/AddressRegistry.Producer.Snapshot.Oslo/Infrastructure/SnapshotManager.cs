namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using Polly;

    public interface ISnapshotManager
    {
        Task<OsloResult?> FindMatchingSnapshot(string persistentLocalId, Instant eventVersion, bool throwStaleWhenGone, CancellationToken ct);
    }

    public sealed class SnapshotManager : ISnapshotManager
    {
        private readonly IPublicApiHttpProxy _publicApiHttpProxy;
        private readonly int _maxRetryWaitIntervalSeconds;
        private readonly int _retryBackoffFactor;

        public SnapshotManager(IPublicApiHttpProxy publicApiHttpProxy, int maxRetryWaitIntervalSeconds, int retryBackoffFactor)
        {
            _publicApiHttpProxy = publicApiHttpProxy;
            _maxRetryWaitIntervalSeconds = maxRetryWaitIntervalSeconds;
            _retryBackoffFactor = retryBackoffFactor;
        }

        public async Task<OsloResult?> FindMatchingSnapshot(string persistentLocalId, Instant eventVersion, bool throwStaleWhenGone, CancellationToken ct)
        {
            return await Policy
                .Handle<Exception>(e => e is StaleSnapshotException or HttpRequestException)
                .WaitAndRetryForeverAsync(retryAttempt =>
                {
                    var waitIntervalSeconds = retryAttempt * _retryBackoffFactor;

                    if (waitIntervalSeconds > _maxRetryWaitIntervalSeconds)
                    {
                        waitIntervalSeconds = _maxRetryWaitIntervalSeconds;
                    }

                    return TimeSpan.FromSeconds(waitIntervalSeconds);
                })
                .ExecuteAsync(async _ => await GetSnapshot(), ct);

            async Task<OsloResult?> GetSnapshot()
            {
                if (ct.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                OsloResult? snapshot;

                try
                {
                    snapshot = await _publicApiHttpProxy.GetSnapshot(persistentLocalId, ct);
                }
                catch (HttpRequestException e)
                {
                    switch (e.StatusCode)
                    {
                        case HttpStatusCode.Gone when throwStaleWhenGone: throw;
                        case HttpStatusCode.Gone: return null;
                        default:
                            throw;
                    }
                }

                var snapshotDto = DateTimeOffset.Parse(snapshot.Identificator.Versie);
                var snapshotVersion = Instant.FromDateTimeOffset(snapshotDto);

                var versionDeltaInSeconds = Math.Floor(eventVersion.Minus(snapshotVersion).TotalSeconds);

                if (versionDeltaInSeconds > 0)
                {
                    throw new StaleSnapshotException();
                }

                return versionDeltaInSeconds == 0
                    ? snapshot
                    : null;
            }
        }
    }
}
