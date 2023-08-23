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
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class SnapshotVerifier<TAggregateRoot, TStreamId> : BackgroundService
        where TAggregateRoot : class, IAggregateRootEntity, ISnapshotable
        where TStreamId : class
    {
        private readonly IHostApplicationLifetime _applicationLifetime;

        private readonly MsSqlSnapshotStoreQueries _snapshotStoreQueries;
        private readonly EventDeserializer _eventDeserializer;
        private readonly EventMapping _eventMapping;
        private readonly IReadonlyStreamStore _streamStore;

        private readonly Func<TAggregateRoot> _aggregateRootFactory;
        private readonly Func<TAggregateRoot, TStreamId> _streamIdFactory;

        private readonly List<string> _membersToIgnore;
        private readonly SnapshotVerificationRepository _snapshotVerificationRepository;
        private readonly ILogger<SnapshotVerifier<TAggregateRoot, TStreamId>> _logger;

        public SnapshotVerifier(
            IHostApplicationLifetime applicationLifetime,
            MsSqlSnapshotStoreQueries snapshotStoreQueries,
            EventDeserializer eventDeserializer,
            EventMapping eventMapping,
            IReadonlyStreamStore streamStore,
            Func<TAggregateRoot> aggregateRootFactory,
            Func<TAggregateRoot, TStreamId> streamIdFactory,
            List<string> membersToIgnore,
            SnapshotVerificationRepository snapshotVerificationRepository,
            ILoggerFactory loggerFactory)
        {
            _applicationLifetime = applicationLifetime;

            _snapshotStoreQueries = snapshotStoreQueries;
            _eventDeserializer = eventDeserializer;
            _eventMapping = eventMapping;
            _streamStore = streamStore;

            _aggregateRootFactory = aggregateRootFactory;
            _streamIdFactory = streamIdFactory;

            _membersToIgnore = membersToIgnore;
            _snapshotVerificationRepository = snapshotVerificationRepository;
            _logger = loggerFactory.CreateLogger<SnapshotVerifier<TAggregateRoot, TStreamId>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await _snapshotStoreQueries.DoesTableExist())
            {
                _logger.LogError("Snapshot table does not exist");
                _applicationLifetime.StopApplication();
                return;
            }

            var lastProcessedSnapshotId = await _snapshotVerificationRepository.MaxSnapshotId(stoppingToken);

            var idsToVerify = (await _snapshotStoreQueries.GetSnapshotIdsToVerify(lastProcessedSnapshotId))?.ToList();
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


                var verificationState = new SnapshotVerificationState(idToVerify.SnapshotId);

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

                await _snapshotVerificationRepository.AddVerificationState(verificationState, stoppingToken);
            }

            _applicationLifetime.StopApplication();
        }

        private async Task<AggregateWithVersion?> GetAggregateBySnapshot(int idToVerify)
        {
            var snapshotBlob = await _snapshotStoreQueries.GetSnapshotBlob(idToVerify);
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
