namespace AddressRegistry.Migrator.Address.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Address.Commands;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.StreetName;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Exceptions;
    using AddressId = AddressRegistry.Address.AddressId;

    internal class StreamMigrator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;
        private readonly ProcessedIdsTable _processedIdsTable;
        private readonly SqlStreamsTable _sqlStreamTable;
        private readonly IList<StreetNameConsumerItem> _consumerItems;
        private readonly bool _skipIncomplete;

        private List<(int processedId, bool isPageCompleted)> _processedIds;

        public StreamMigrator(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ILifetimeScope lifetimeScope,
            IList<StreetNameConsumerItem> consumerItems)
        {
            _logger = loggerFactory.CreateLogger("AddressMigrator");
            _lifetimeScope = lifetimeScope;
            _consumerItems = consumerItems;

            var connectionString = configuration.GetConnectionString("events");
            _processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            _sqlStreamTable = new SqlStreamsTable(connectionString);

            _skipIncomplete = bool.Parse(configuration["SkipIncomplete"]);
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            await _processedIdsTable.CreateTableIfNotExists();

            var processedIdsList = await _processedIdsTable.GetProcessedIds();
            _processedIds = new List<(int, bool)>(processedIdsList);

            var lastCursorPosition = _processedIds.Any()
                ? _processedIds
                    .Where(x => x.isPageCompleted)
                    .Select(x => x.processedId)
                    .DefaultIfEmpty(0)
                    .Max()
                : 0;

            var pageOfStreams = (await _sqlStreamTable.ReadNextAddressStreamPage(lastCursorPosition)).ToList();

            while (pageOfStreams.Any() && !ct.IsCancellationRequested)
            {
                try
                {
                    var processedPageItems = await ProcessStreams(pageOfStreams, ct);

                    if (!processedPageItems.Any())
                    {
                        lastCursorPosition = pageOfStreams.Max(x => x.internalId);
                    }
                    else
                    {
                        await _processedIdsTable.CompletePageAsync(pageOfStreams.Select(x => x.internalId).ToList());
                        processedPageItems.ForEach(x => _processedIds.Add((x, true)));
                        lastCursorPosition = _processedIds.Max(x => x.processedId);
                    }

                    pageOfStreams = (await _sqlStreamTable.ReadNextAddressStreamPage(lastCursorPosition)).ToList();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ProcessStreams cancelled.");
                }
            }

            var retryProcessedIdsList = (await _processedIdsTable.GetRetryProcessedIds()).ToList();

            var retryProcessedIdsToProcess = retryProcessedIdsList
                .Where(x => !x.isPageCompleted)
                .ToList();

            foreach (var retryProcessedId in retryProcessedIdsToProcess)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var processedPageItems = await ProcessStreams(new (int, string)[] { (retryProcessedId.processedId, retryProcessedId.aggregateId) }, ct);

                    await _processedIdsTable.CompleteRetryPageAsync(processedPageItems);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ProcessStreams cancelled.");
                }
            }
        }

        private async Task<List<int>> ProcessStreams(IEnumerable<(int, string)> streamsToProcess, CancellationToken ct, bool retry = false)
        {
            var processedItems = new ConcurrentBag<int>();
            var migrateCommands = new ConcurrentDictionary<int, MigrateAddressToStreetName>();

            var backOfficeContextFactory = _lifetimeScope.Resolve<IDbContextFactory<BackOfficeContext>>();

            await Parallel.ForEachAsync(streamsToProcess, ct, async (stream, innerCt) =>
            {
                try
                {
                    var command = await CreateMigrateCommand(stream, innerCt);
                    if (command is not null)
                    {
                        migrateCommands.TryAdd(stream.Item1, command);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        ex,
                        $"Unexpected exception for creating command stream '{stream.Item1}', aggregateId '{stream.Item2}'");
                    throw;
                }
            });

            var migrateCommandsByStreetName = migrateCommands.GroupBy(x => x.Value.StreetNamePersistentLocalId);

            await Parallel.ForEachAsync(migrateCommandsByStreetName, ct, async (commands, innerCt) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(innerCt);

                foreach (var (internalId, command)  in commands.OrderBy(x => x.Key))
                {
                    try
                    {
                        await CreateAndDispatchCommand(command, innerCt);

                        await _processedIdsTable.Add(internalId);
                        processedItems.Add(internalId);

                        await backOfficeContext
                            .AddressPersistentIdStreetNamePersistentIds
                            .AddAsync(
                                new AddressPersistentIdStreetNamePersistentId(command.AddressPersistentLocalId,
                                    command.StreetNamePersistentLocalId), innerCt);
                        await backOfficeContext.SaveChangesAsync(innerCt);
                    }
                    catch (ParentAddressNotFoundException)
                    {
                        _logger.LogWarning($"Parent not found for child '{internalId}'.");

                        if (_skipIncomplete)
                            continue;

                        if (retry)
                            throw;

                        await _processedIdsTable.Add(internalId);
                        await _processedIdsTable.AddRetry(internalId, command.AddressId.ToString());
                        processedItems.Add(internalId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(
                            ex,
                            $"Unexpected exception for migration stream '{internalId}', aggregateId '{command.AddressId}'");
                        throw;
                    }
                }
            });

            return processedItems.ToList();
        }

        private async Task<MigrateAddressToStreetName?>  CreateMigrateCommand(
            (int, string) stream,
            CancellationToken token)
        {
            var (internalId, aggregateId) = stream;

            if (token.IsCancellationRequested)
            {
                return null;
            }

            if (_processedIds.Contains((internalId, false)))
            {
                _logger.LogDebug($"Already migrated '{internalId}', skipping...");
                return null;
            }

            await using var streamLifetimeScope = _lifetimeScope.BeginLifetimeScope();

            var addressRepo = streamLifetimeScope.Resolve<IAddresses>();
            var addressId = new AddressId(Guid.Parse(aggregateId));
            var addressAggregate = await addressRepo.GetAsync(addressId, token);

            var streetNameId = (Guid)addressAggregate.StreetNameId;
            var streetName = _consumerItems.SingleOrDefault(x => x.StreetNameId == streetNameId);

            if (!addressAggregate.IsComplete && (addressAggregate.Geometry is null || addressAggregate.Status is null))
            {
                if (addressAggregate.IsRemoved)
                {
                    _logger.LogDebug($"Skipping incomplete & removed Address '{aggregateId}'.");
                    return null;
                }

                var streetNameAggregate = await streamLifetimeScope
                    .Resolve<IStreetNames>()
                    .GetAsync(new StreetNameStreamId(new StreetNamePersistentLocalId(streetName.PersistentLocalId)), token);

                if (_skipIncomplete || !RegionFilter.IsFlemishRegion(streetNameAggregate.MigratedNisCode!))
                {
                    return null;
                }

                throw new InvalidOperationException($"Incomplete but not removed Address '{aggregateId}'.");
            }

            if (streetName == null)
            {
                throw new InvalidOperationException($"StreetName for StreetNameId '{streetNameId}' was not found.");
            }

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(streetName.PersistentLocalId);
            var migrateCommand = addressAggregate.CreateMigrateCommand(streetNamePersistentLocalId);

            return migrateCommand;
        }

        private async Task CreateAndDispatchCommand(
            MigrateAddressToStreetName migrateCommand,
            CancellationToken ct)
        {
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
