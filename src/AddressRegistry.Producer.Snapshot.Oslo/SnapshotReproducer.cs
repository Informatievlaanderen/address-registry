namespace AddressRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Dapper;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using Npgsql;

    public class SnapshotReproducer : BackgroundService
    {
        private readonly string _integrationConnectionString;
        private readonly IOsloProxy _osloProxy;
        private readonly IProducer _producer;
        private readonly IClock _clock;
        private readonly INotificationService _notificationService;
        private readonly int _utcHourToRunWithin;
        private readonly ILogger<SnapshotReproducer> _logger;

        public SnapshotReproducer(
            string integrationConnectionString,
            IOsloProxy osloProxy,
            IProducer producer,
            IClock clock,
            INotificationService notificationService,
            int utcHourToRunWithin,
            ILoggerFactory loggerFactory)
        {
            _integrationConnectionString = integrationConnectionString;
            _osloProxy = osloProxy;
            _producer = producer;
            _notificationService = notificationService;
            _utcHourToRunWithin = utcHourToRunWithin;
            _clock = clock;

            _logger = loggerFactory.CreateLogger<SnapshotReproducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = _clock.GetCurrentInstant().ToDateTimeUtc();
                if (now.Hour == _utcHourToRunWithin)
                {
                    _logger.LogInformation($"Starting {GetType().Name}");

                    try
                    {
                        //execute query
                        var idsToProcess = GetIdsToProcess(now);

                        //reproduce
                        foreach (var id in idsToProcess)
                        {
                            await FindAndProduce(async () =>
                                    await _osloProxy.GetSnapshot(id.PersistentLocalId.ToString(), stoppingToken),
                                id.Position,
                                stoppingToken);
                        }

                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);

                        await _notificationService.PublishToTopicAsync(new NotificationMessage(
                            GetType().Name,
                            $"Reproducing snapshot failed: {ex}",
                            GetType().Name,
                            NotificationSeverity.Danger));
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        protected async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, long storePosition, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.Identificator.ObjectId, result.JsonContent, storePosition, ct);
            }
        }

        protected async Task Produce(string puri, string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        protected List<(int PersistentLocalId, long Position)> GetIdsToProcess(DateTime utcNow)
        {
            using var connection = new NpgsqlConnection(_integrationConnectionString);

            var todayMidnight = utcNow.Date;
            var yesterdayMidnight = todayMidnight.AddDays(-1);

            var records = connection.Query<AddressPosition>(
                $"""
                 SELECT persistent_local_id, position, version_timestamp
                 FROM integration_address.address_versions
                 where version_timestamp >= '{yesterdayMidnight:yyyy-MM-dd}' and version_timestamp < '{todayMidnight:yyyy-MM-dd}'
                 """);

            var duplicateEvents = records
                .GroupBy(x => new
                {
                    AddressPersistentLocalId = x.address_persistent_local_id,
                    TimeStamp = x.version_timestamp.ToString("yyyyMMddHHmmss") // Format the timestamp to seconds as OSLO API doesn't return the milliseconds of the timestamp
                })
                .Where(x => x.Count() > 1)
                .Select(x =>
                {
                    var latest = x.MaxBy(y => y.position)!;
                    return (persistent_local_id: latest.address_persistent_local_id, latest.position);
                })
                .ToList();

            return duplicateEvents;
        }

        private sealed class AddressPosition
        {
            public int address_persistent_local_id { get; init; }
            public long position { get; init; }
            public DateTimeOffset version_timestamp { get; init; }
        }
    }
}

