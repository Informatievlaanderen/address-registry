namespace AddressRegistry.Snapshot.Verifier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class SnapshotVerifier<TAggregateRoot, TStreamId> : BackgroundService
        where TAggregateRoot : class, IAggregateRootEntity, ISnapshotable
        where TStreamId : class
    {
        private readonly IHostApplicationLifetime _applicationLifetime;

        private readonly MsSqlSnapshotStoreVerifier _snapshotStoreVerifier;
        private readonly EventDeserializer _eventDeserializer;
        private readonly EventMapping _eventMapping;
        private readonly IReadonlyStreamStore _streamStore;

        private readonly Func<TAggregateRoot> _aggregateRootFactory;
        private readonly Func<TAggregateRoot, TStreamId> _streamIdFactory;

        private readonly List<string> _membersToIgnore;
        private readonly IDbContextFactory<SnapshotVerifierContext> _snapshotVerifierContextFactory;
        private readonly ILogger<SnapshotVerifier<TAggregateRoot, TStreamId>> _logger;

        public SnapshotVerifier(
            IHostApplicationLifetime applicationLifetime,
            MsSqlSnapshotStoreVerifier snapshotStoreVerifier,
            EventDeserializer eventDeserializer,
            EventMapping eventMapping,
            IReadonlyStreamStore streamStore,
            Func<TAggregateRoot> aggregateRootFactory,
            Func<TAggregateRoot, TStreamId> streamIdFactory,
            List<string> membersToIgnore,
            IDbContextFactory<SnapshotVerifierContext> snapshotVerifierContextFactory,
            ILoggerFactory loggerFactory)
        {
            _applicationLifetime = applicationLifetime;

            _snapshotStoreVerifier = snapshotStoreVerifier;
            _eventDeserializer = eventDeserializer;
            _eventMapping = eventMapping;
            _streamStore = streamStore;

            _aggregateRootFactory = aggregateRootFactory;
            _streamIdFactory = streamIdFactory;

            _membersToIgnore = membersToIgnore;
            _snapshotVerifierContextFactory = snapshotVerifierContextFactory;
            _logger = loggerFactory.CreateLogger<SnapshotVerifier<TAggregateRoot, TStreamId>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await _snapshotStoreVerifier.DoesTableExist())
            {
                _logger.LogError("Snapshot table does not exist");
                _applicationLifetime.StopApplication();
                return;
            }

            int? lastProcessedSnapshotId = null;
            await using (var context = await _snapshotVerifierContextFactory.CreateDbContextAsync(stoppingToken))
            {
                if (context.VerificationStates.Any())
                {
                    lastProcessedSnapshotId = context.VerificationStates.Max(x => x.SnapshotId);
                }
            }

            var idsToVerify = (await _snapshotStoreVerifier.GetSnapshotIdsToVerify(lastProcessedSnapshotId))?.ToList();
            if (idsToVerify is null || !idsToVerify.Any())
            {
                _logger.LogInformation("Could not retrieve snapshot ids to verify");
                _applicationLifetime.StopApplication();
                return;
            }

            foreach (var idToVerify in idsToVerify)
            {
                var aggregateBySnapshot = await GetAggregateBySnapshot(idToVerify.SnapshotId);
                if (aggregateBySnapshot is null)
                {
                    _logger.LogError("Could not retrieve snapshot blob for snapshot id {SnapshotId}",
                        idToVerify.SnapshotId);
                    continue;
                }

                var aggregateByEvents = await GetAggregateByEvents(_streamIdFactory(aggregateBySnapshot.Aggregate),
                    (int)aggregateBySnapshot.StreamVersion, stoppingToken);
                if (aggregateByEvents is null)
                {
                    _logger.LogError("Could not retrieve stream from stream store for {StreamId}", idToVerify.StreamId);
                    continue;
                }

                var compareLogic = new CompareLogic(new ComparisonConfig
                {
                    MembersToIgnore = new List<string> { "_recorder", "_router", "_applier", "_lastEvent", "Strategy" }
                        .Concat(_membersToIgnore).ToList()
                });

                await using var context = await _snapshotVerifierContextFactory.CreateDbContextAsync(stoppingToken);
                var verificationState = new SnapshotVerificationState { SnapshotId = idToVerify.SnapshotId };

                var comparisonResult = compareLogic.Compare(aggregateBySnapshot.Aggregate, aggregateByEvents);
                if (!comparisonResult.AreEqual)
                {
                    _logger.LogError("Snapshot {SnapshotId} does not match aggregate from events", idToVerify.SnapshotId);
                    verificationState.Status = SnapshotStateStatus.Failed;
                    verificationState.Differences = comparisonResult.DifferencesString;
                    //TODO: Notify
                }
                else
                {
                    verificationState.Status = SnapshotStateStatus.Verified;
                }

                await context.VerificationStates.AddAsync(verificationState, stoppingToken);
                await context.SaveChangesAsync(stoppingToken);
            }

            _applicationLifetime.StopApplication();
        }

        private async Task<AggregateWithVersion?> GetAggregateBySnapshot(int idToVerify)
        {
            var snapshotBlob = await _snapshotStoreVerifier.GetSnapshotBlob(idToVerify);
            if (snapshotBlob is null)
            {
                return null;
            }

            var snapshotContainer =
                (SnapshotContainer)_eventDeserializer.DeserializeObject(snapshotBlob, typeof(SnapshotContainer));
            var snapshotType = _eventMapping.GetEventType(snapshotContainer.Info.Type);
            var snapshot = _eventDeserializer.DeserializeObject(snapshotContainer.Data, snapshotType);

            var snapshotAggregate = _aggregateRootFactory.Invoke();
            snapshotAggregate.RestoreSnapshot(snapshot);
            return new AggregateWithVersion(snapshotAggregate, snapshotContainer.Info.StreamVersion);
        }

        private async Task<TAggregateRoot?> GetAggregateByEvents(
            TStreamId streamId,
            int snapshotAggregateStreamVersion,
            CancellationToken stoppingToken)
        {
            var page = await _streamStore.ReadStreamBackwards(streamId.ToString(), snapshotAggregateStreamVersion, 100,
                stoppingToken);
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                return null;
            }

            var aggregate = _aggregateRootFactory.Invoke();
            var events = new List<object>();
            events.AddRange(await ParseEvents(page, stoppingToken));
            while (!page.IsEnd)
            {
                page = await page.ReadNext(stoppingToken);
                events.AddRange(await ParseEvents(page, stoppingToken));
            }

            events.Reverse(); // events are read backwards, so reverse them to get them in the correct order
            aggregate.Initialize(events);

            return aggregate;
        }

        private async Task<IEnumerable<object>> ParseEvents(
            ReadStreamPage page,
            CancellationToken cancellationToken)
        {
            var events = new List<object>();

            foreach (var message in page.Messages)
            {
                var eventType = _eventMapping.GetEventType(message.Type);
                var eventData = await message.GetJsonData(cancellationToken);
                events.Add(_eventDeserializer.DeserializeObject(eventData, eventType));
            }

            return events;
        }

        private record AggregateWithVersion(TAggregateRoot Aggregate, long StreamVersion);
    }
}
