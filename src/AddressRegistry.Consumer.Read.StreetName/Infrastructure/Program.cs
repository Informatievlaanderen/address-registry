namespace AddressRegistry.Consumer.Read.StreetName.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Destructurama;
    using Elastic.Clients.Elasticsearch;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Municipality;
    using Municipality.Infrastructure.Modules;
    using Projections.Elastic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;
    using SqlStreamStore;

    public sealed class Program
    {
        private Program() { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting AddressRegistry.Consumer.Read.StreetName");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true,
                            reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    SelfLog.Enable(Console.WriteLine);

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes()
                        .CreateLogger();

                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);
                    services.AddDbContextFactory<MunicipalityConsumerContext>();

                    services
                        .AddDbContextFactory<StreetNameConsumerContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("ConsumerStreetName"),
                                sqlServerOptions =>
                                {
                                    sqlServerOptions.EnableRetryOnFailure();
                                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadStreetName,
                                        Schema.ConsumerReadStreetName);
                                }), ServiceLifetime.Transient);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder.Register(c => new Lazy<IPersistentLocalIdGenerator>(() => throw new NotImplementedException("IPersistentLocalIdGenerator not implemented in consumer streetname.")));
                    builder
                        .Register(c =>
                        {
                            var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                            var topic = $"{hostContext.Configuration["Topic"]}" ?? throw new ArgumentException("Configuration has no StreetNameTopic.");
                            var suffix = hostContext.Configuration["ConsumerGroupSuffix"];
                            var consumerGroupId = $"AddressRegistry.StreetNameLatestItemConsumer.{topic}{suffix}";

                            var consumerOptions = new ConsumerOptions(
                                new BootstrapServers(bootstrapServers),
                                new Topic(topic),
                                new ConsumerGroupId(consumerGroupId),
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                                hostContext.Configuration["Kafka:SaslUserName"],
                                hostContext.Configuration["Kafka:SaslPassword"]));

                            var offset = hostContext.Configuration["ConsumerOffset"];

                            if (!string.IsNullOrWhiteSpace(offset) && long.TryParse(offset, out var result))
                            {
                                var ignoreDataCheck = hostContext.Configuration.GetValue<bool>("IgnoreConsumerOffsetDataCheck", false);

                                if (!ignoreDataCheck)
                                {
                                    using var ctx = c.Resolve<StreetNameConsumerContext>();

                                    if (ctx.StreetNameLatestItems.Any())
                                    {
                                        throw new InvalidOperationException(
                                            $"Cannot set Kafka offset to {offset} because {nameof(ctx.StreetNameLatestItems)} has data.");
                                    }
                                }

                                consumerOptions.ConfigureOffset(new Offset(result));
                            }

                            return new IdempotentConsumer<StreetNameConsumerContext>(
                                consumerOptions,
                                c.Resolve<IDbContextFactory<StreetNameConsumerContext>>(),
                                c.Resolve<ILoggerFactory>());
                        })
                        .As<IIdempotentConsumer<StreetNameConsumerContext>>()
                        .SingleInstance();

                    builder
                        .Register(c =>
                        {
                            var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                            var topic = $"{hostContext.Configuration["Topic"]}" ?? throw new ArgumentException("Configuration has no StreetNameTopic Topic.");
                            var suffix = hostContext.Configuration["ConsumerGroupSuffix"];
                            var consumerGroupId = $"AddressRegistry.StreetNameElasticConsumer.{topic}{suffix}";

                            var consumerOptions = new ConsumerOptions(
                                new BootstrapServers(bootstrapServers),
                                new Topic(topic),
                                new ConsumerGroupId(consumerGroupId),
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                                hostContext.Configuration["Kafka:SaslUserName"],
                                hostContext.Configuration["Kafka:SaslPassword"]));

                            var offset = hostContext.Configuration["ConsumerOffset"];

                            if (!string.IsNullOrWhiteSpace(offset) && long.TryParse(offset, out var result))
                            {
                                consumerOptions.ConfigureOffset(new Offset(result));
                            }

                            return new Consumer(consumerOptions, c.Resolve<ILoggerFactory>());
                        })
                        .Keyed<IConsumer>(nameof(StreetNameElasticConsumer))
                        .SingleInstance();

                    builder
                        .RegisterModule(new ElasticModule(hostContext.Configuration))
                        .RegisterModule(new MunicipalityConsumerModule(hostContext.Configuration, services, loggerFactory))
                        .RegisterModule(new CommandHandlingModule(hostContext.Configuration));

                    builder.RegisterSnapshotModule(hostContext.Configuration);

                    builder
                        .RegisterType<StreetNameLatestItemConsumer>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder
                        .RegisterType<StreetNameElasticConsumer>()
                        .WithAttributeFiltering()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var elasticsearchClient = host.Services.GetRequiredService<ElasticsearchClient>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                        async () =>
                        {
                            AddressRegistry.Infrastructure.MigrationsHelper.EnsureSqlStreamStoreSchema<Program>(host.Services.GetRequiredService<MsSqlStreamStore>(), loggerFactory);
                            AddressRegistry.Infrastructure.MigrationsHelper.EnsureSqlSnapshotStoreSchema<Program>(host.Services.GetRequiredService<MsSqlSnapshotStore>(), loggerFactory);
                            AddressRegistry.Infrastructure.MigrationsHelper.Run(configuration.GetConnectionString("Sequences"), loggerFactory);

                            await MigrationsHelper.RunAsync(
                                configuration.GetConnectionString("ConsumerStreetNameAdmin"),
                                loggerFactory,
                                CancellationToken.None);

                            var elasticIndex = new StreetNameElasticIndex(
                                elasticsearchClient,
                                configuration);

                            await elasticIndex.CreateIndexIfNotExist(CancellationToken.None);
                            await elasticIndex.CreateAliasIfNotExist(CancellationToken.None);

                            await host.RunAsync().ConfigureAwait(false);
                        },
                        DistributedLockOptions.LoadFromConfiguration(configuration),
                        logger)
                    .ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(500, default);
                throw;
            }
            finally
            {
                logger.LogInformation("Stopping...");
            }
        }
    }
}
