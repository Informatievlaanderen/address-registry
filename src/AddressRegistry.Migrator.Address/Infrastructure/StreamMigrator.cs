using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressRegistry.Migrator.Address.Infrastructure
{
    using System.Collections.Concurrent;
    using System.Threading;
    using AddressRegistry.Address;
    using AddressRegistry.Address.Commands;
    using Api.BackOffice;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Consumer;
    using Consumer.StreetName;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using StreetName;
    using StreetName.Exceptions;
    using AddressId = AddressRegistry.Address.AddressId;

    internal class StreamMigrator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private ILogger _logger;

        private ProcessedIdsTable _processedIdsTable;
        private SqlStreamsTable _sqlStreamTable;
        private IAddresses _addressRepo;
        private BackOfficeContext _backOfficeContext;
        private ConsumerContext _consumerContext;

        public StreamMigrator(ILoggerFactory loggerFactory, IConfiguration configuration, ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            _logger = loggerFactory.CreateLogger("AddressMigrator");

            var connectionString = configuration.GetConnectionString("events");
            _processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            _sqlStreamTable = new SqlStreamsTable(connectionString);

            _addressRepo = lifetimeScope.Resolve<IAddresses>();
            _backOfficeContext = lifetimeScope.Resolve<BackOfficeContext>();
            _consumerContext = lifetimeScope.Resolve<ConsumerContext>();
        }

        public async Task Process(CancellationToken ct)
        {
            await _processedIdsTable.CreateTableIfNotExists();

            var processedIds = (await _processedIdsTable.GetProcessedIds())?.ToList() ?? new List<int>();
            var lastCursorPosition = processedIds.Any() ? processedIds.Max() : 0;

            var streams = (await _sqlStreamTable.ReadNextAddressStreamPage(lastCursorPosition))?.ToList() ?? new List<(int, string)>();

            while (streams.Any())
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                await ProcessStreams(streams, processedIds, ct);

                lastCursorPosition = streams.Max(x => x.internalId);
                streams = (await _sqlStreamTable.ReadNextAddressStreamPage(lastCursorPosition))?.ToList() ?? new List<(int, string)>();
            }
        }

        private async Task ProcessStreams(IEnumerable<(int, string)> streamsToProcess, List<int> processedIds, CancellationToken ct)
        {
            var parentNotFoundCollection = new BlockingCollection<(int, string)>();

            await Parallel.ForEachAsync(streamsToProcess, ct, async (stream, token) =>
            {
                await ProcessStream(processedIds, ct, stream, token, parentNotFoundCollection);
            });

            foreach (var failedChildAddress in parentNotFoundCollection)
            {
                await ProcessStream(processedIds, ct, processedIds, );
            }
        }

        private async Task ProcessStream(List<int> processedIds, CancellationToken ct, (int, string) stream, CancellationToken token,
            BlockingCollection<(int, string)> parentNotFoundCollection)
        {
            var (internalId, aggregateId) = stream;

            try
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (processedIds.Contains(internalId))
                {
                    _logger.LogDebug($"Already migrated '{internalId}', skipping...");
                    return;
                }

                // TODO: persist processed stream pages (min - max) timestamp

                var addressId = new AddressId(Guid.Parse(aggregateId));
                var addressAggregate = await _addressRepo.GetAsync(addressId, ct);

                if (!addressAggregate.IsComplete)
                {
                    if (addressAggregate.IsRemoved)
                    {
                        _logger.LogDebug($"Skipping incomplete & removed Address '{aggregateId}'.");
                        return;
                    }

                    throw new InvalidOperationException($"Incomplete but not removed Address '{aggregateId}'.");
                }

                var streetNameId = (Guid) addressAggregate.StreetNameId;
                var streetName =
                    await _consumerContext.StreetNameConsumerItems.FindAsync(new object[] { streetNameId }, ct);

                if (streetName == null)
                {
                    throw new InvalidOperationException(
                        $"StreetName for StreetNameId '{streetNameId}' was not found.");
                }

                await CreateAndDispatchCommand(streetName, addressAggregate, ct);

                await _processedIdsTable.Add(internalId);
                processedIds.Add(internalId);

                await _backOfficeContext
                    .AddressPersistentIdStreetNamePersistentId
                    .AddAsync(
                        new AddressPersistentIdStreetNamePersistentId(addressAggregate.PersistentLocalId,
                            streetName.PersistentLocalId), ct);
                await _backOfficeContext.SaveChangesAsync(ct);
            }
            catch (ParentAddressNotFoundException ex)
            {
                parentNotFoundCollection.Add(stream, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // insert into table
            }

        }

        private async Task CreateAndDispatchCommand(
            StreetNameConsumerItem streetName,
            Address address,
            CancellationToken ct)
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(streetName.PersistentLocalId);
            var migrateCommand = address.CreateMigrateCommand(streetNamePersistentLocalId);
            var markMigrated = new MarkAddressAsMigrated(
                new AddressId(migrateCommand.AddressId),
                migrateCommand.StreetNamePersistentLocalId,
                migrateCommand.Provenance);

            await using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    markMigrated.CreateCommandId(),
                    markMigrated,
                    cancellationToken: ct);
            }

            await using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    migrateCommand.CreateCommandId(),
                    migrateCommand,
                    cancellationToken: ct);
            }
        }
    }
}
