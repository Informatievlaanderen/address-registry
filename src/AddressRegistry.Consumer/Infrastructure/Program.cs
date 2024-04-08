namespace AddressRegistry.Consumer.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Destructurama;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;
    using SqlStreamStore;
    using Consumer = AddressRegistry.Consumer.Consumer;

    public sealed class Program
    {
        private Program()
        { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Initializing AddressRegistry.Consumer");

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

                    Log.Logger = new LoggerConfiguration() //NOSONAR logging configuration is safe
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
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger); //NOSONAR logging configuration is safe

                    var connectionString = hostContext.Configuration.GetConnectionString("Consumer");
                    services
                        .AddDbContextFactory<IdempotentConsumerContext>((_, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(connectionString,
                                sqlServerOptions =>
                                {
                                    sqlServerOptions.EnableRetryOnFailure();
                                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Consumer, Schema.Consumer);
                                }));
                    services
                        .AddDbContext<ConsumerContext>((_, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(connectionString, sqlServerOptions =>
                            {
                                sqlServerOptions.EnableRetryOnFailure();
                                sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerProjections, Schema.ConsumerProjections);
                            }));

                    services.ConfigureIdempotency(
                        hostContext.Configuration
                            .GetSection(IdempotencyConfiguration.Section)
                            .Get<IdempotencyConfiguration>()
                            .ConnectionString!,
                        new IdempotencyMigrationsTableInfo(Schema.Import),
                        new IdempotencyTableInfo(Schema.Import),
                        loggerFactory);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger); //NOSONAR logging configuration is safe

                    containerBuilder.Register(c =>
                    {
                        var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                        var topic = $"{hostContext.Configuration["StreetNameTopic"]}" ?? throw new ArgumentException("Configuration has no StreetNameTopic.");
                        var suffix = hostContext.Configuration["StreetNameConsumerGroupSuffix"];
                        var consumerGroupId = $"AddressRegistry.Consumer.{topic}{suffix}";

                        var consumerOptions = new ConsumerOptions(
                            new BootstrapServers(bootstrapServers),
                            new Topic(topic),
                            new ConsumerGroupId(consumerGroupId),
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                        consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                            hostContext.Configuration["Kafka:SaslUserName"],
                            hostContext.Configuration["Kafka:SaslPassword"]));

                        var offset = hostContext.Configuration["StreetNameTopicOffset"];

                        if (!string.IsNullOrWhiteSpace(offset) && long.TryParse(offset, out var result))
                        {
                            var ignoreDataCheck = hostContext.Configuration.GetValue<bool>("IgnoreStreetNameTopicOffsetDataCheck", false);

                            if (!ignoreDataCheck)
                            {
                                using var ctx = c.Resolve<ConsumerContext>();

                                if (ctx.StreetNameConsumerItems.Any())
                                {
                                    throw new InvalidOperationException(
                                        $"Cannot set Kafka offset to {offset} because {nameof(ctx.StreetNameConsumerItems)} has data.");
                                }
                            }

                            consumerOptions.ConfigureOffset(new Offset(result));
                        }

                        return consumerOptions;
                    });

                    containerBuilder.RegisterType<IdempotentCommandHandler>()
                        .As<IIdempotentCommandHandler>()
                        .AsSelf()
                        .InstancePerLifetimeScope();

                    containerBuilder
                        .RegisterType<IdempotentConsumer<IdempotentConsumerContext>>()
                        .As<IIdempotentConsumer<IdempotentConsumerContext>>()
                        .SingleInstance();

                    containerBuilder
                        .RegisterType<Consumer>()
                        .As<IHostedService>()
                        .SingleInstance();

                    containerBuilder.RegisterModule(new ApiModule(hostContext.Configuration, services, loggerFactory));

                    containerBuilder
                        .RegisterType<ConsumerProjections>()
                        .As<IHostedService>()
                        .SingleInstance();

                    containerBuilder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting AddressRegistry.Consumer");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                        async () =>
                        {
                            AddressRegistry.Infrastructure.MigrationsHelper.EnsureSqlStreamStoreSchema<Program>(host.Services.GetRequiredService<MsSqlStreamStore>(), loggerFactory);
                            AddressRegistry.Infrastructure.MigrationsHelper.EnsureSqlSnapshotStoreSchema<Program>(host.Services.GetRequiredService<MsSqlSnapshotStore>(), loggerFactory);

                            await MigrationsHelper.RunAsync(
                                configuration.GetConnectionString("ConsumerAdmin"),
                                loggerFactory,
                                CancellationToken.None);

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

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));
            builder.RegisterModule(new ConsumerModule(configuration, services, loggerFactory));
            builder.RegisterModule(new ProjectorModule(configuration));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
