namespace AddressRegistry.Migrator.Address.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Address.Commands;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Consumer;
    using Consumer.StreetName;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Polly;
    using Serilog;
    using StreetName;
    using AddressId = AddressRegistry.Address.AddressId;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting StreetNameRegistry.Consumer");

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await Policy
                                    .Handle<SqlException>()
                                    .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(60),
                                        (_, timespan) => Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                                    .ExecuteAsync(async () =>
                                    {
                                        await ProcessStreams(container, configuration, ct);
                                    });
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetService<ILogger<Program>>()!);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(1000, default);
                throw;
            }

            Log.Information("Stopping...");
            Closing.Close();
        }

        private static async Task ProcessStreams(
            IServiceProvider container,
            IConfigurationRoot configuration,
            CancellationToken ct)
        {
            var loggerFactory = container.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("AddressMigrator");

            var connectionString = configuration.GetConnectionString("events");
            var processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            await processedIdsTable.CreateTableIfNotExists();
            var processedIds = (await processedIdsTable.GetProcessedIds())?.ToList() ?? new List<int>();
            var lastCursorPosition = processedIds.Any() ? processedIds.Max() : 0;

            var actualContainer = container.GetRequiredService<ILifetimeScope>();

            var addressRepo = actualContainer.Resolve<IAddresses>();
            
            //var backOfficeContext = actualContainer.Resolve<BackOfficeContext>();
            var consumerContext = actualContainer.Resolve<ConsumerContext>();
            var sqlStreamTable = new SqlStreamsTable(connectionString);

            var streams = (await sqlStreamTable.ReadNextAddressStreamPage(lastCursorPosition))?.ToList() ?? new List<(int, string)>();

            async Task<bool> ProcessStream(IEnumerable<(int, string)> streamsToProcess)
            {
                if (CancellationTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                foreach (var (internalId, aggregateId) in streamsToProcess)
                {
                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        return false;
                    }

                    if (processedIds.Contains(internalId))
                    {
                        logger.LogDebug($"Already migrated '{internalId}', skipping...");
                        continue;
                    }

                    var addressId = new AddressId(Guid.Parse(aggregateId));
                    var addressAggregate = await addressRepo.GetAsync(addressId, ct);

                    if (!addressAggregate.IsComplete)
                    {
                        if (addressAggregate.IsRemoved)
                        {
                            logger.LogDebug($"Skipping incomplete & removed Address '{aggregateId}'.");
                            continue;
                        }

                        throw new InvalidOperationException($"Incomplete but not removed Address '{aggregateId}'.");
                    }

                    var streetNameId = (Guid)addressAggregate.StreetNameId;
                    var streetName =
                        await consumerContext.StreetNameConsumerItems.FindAsync(new object[] {streetNameId}, ct);

                    if (streetName == null)
                    {
                        throw new InvalidOperationException(
                            $"StreetName for StreetNameId '{streetNameId}' was not found.");
                    }

                    await CreateAndDispatchCommand(streetName, addressAggregate, actualContainer, ct);

                    await processedIdsTable.Add(internalId);
                    processedIds.Add(internalId);

                    //await backOfficeContext
                    //    .MunicipalityIdByPersistentLocalId
                    //    .AddAsync(new MunicipalityIdByPersistentLocalId(streetName.PersistentLocalId, municipality.MunicipalityId), ct);
                    //await backOfficeContext.SaveChangesAsync(ct);
                }

                return true;
            }

            while (streams.Any())
            {
                if (!await ProcessStream(streams))
                {
                    break;
                }

                lastCursorPosition = streams.Max(x => x.internalId);
                streams = (await sqlStreamTable.ReadNextAddressStreamPage(lastCursorPosition))?.ToList() ?? new List<(int, string)>();
            }
        }

        private static async Task CreateAndDispatchCommand(
            StreetNameConsumerItem streetName,
            Address address,
            ILifetimeScope actualContainer,
            CancellationToken ct)
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(streetName.PersistentLocalId);
            var migrateCommand = address.CreateMigrateCommand(streetNamePersistentLocalId);
            var markMigrated = new MarkAddressAsMigrated(
                new AddressId(migrateCommand.AddressId),
                migrateCommand.StreetNamePersistentLocalId,
                migrateCommand.Provenance);

            await using (var scope = actualContainer.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    markMigrated.CreateCommandId(),
                    markMigrated,
                    cancellationToken: ct);
            }

            await using (var scope = actualContainer.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    migrateCommand.CreateCommandId(),
                    migrateCommand,
                    cancellationToken: ct);
            }
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));

            builder.RegisterModule(new ProjectorModule(configuration));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
