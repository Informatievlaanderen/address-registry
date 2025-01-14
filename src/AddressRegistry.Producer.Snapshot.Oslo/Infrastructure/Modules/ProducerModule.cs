namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure.Modules
{
    using System;
    using AddressRegistry.Infrastructure;
    using Amazon.SimpleNotificationService;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class ProducerModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ProducerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => SystemClock.Instance)
                .As<IClock>()
                .SingleInstance();

            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            _services.AddOsloProxy(_configuration["OsloApiUrl"]);
            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterModule<EnvelopeModule>()
                .RegisterEventstreamModule(_configuration)
                .RegisterModule(new ProjectorModule(_configuration));

            RegisterProjections(builder);
            RegisterReproducers();
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            var logger = _loggerFactory.CreateLogger<ProducerModule>();
            var connectionString = _configuration.GetConnectionString("ProducerProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(_services, _loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(_services, _loggerFactory, logger);
            }

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ProducerContext), Schema.ProducerSnapshotOslo, MigrationTables.ProducerSnapshotOslo);

            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerProjections, ProducerContext>(c =>
                    {
                        var osloNamespace = _configuration["OsloNamespace"].TrimEnd('/');

                        return new ProducerProjections(
                            new Producer(CreateProducerOptions()),
                            new SnapshotManager(
                                c.Resolve<ILoggerFactory>(),
                                c.Resolve<IOsloProxy>(),
                                SnapshotManagerOptions.Create(
                                    _configuration["RetryPolicy:MaxRetryWaitIntervalSeconds"],
                                    _configuration["RetryPolicy:RetryBackoffFactor"])),
                            osloNamespace);
                    },
                    connectedProjectionSettings);
        }

        private void RegisterReproducers()
        {
            _services.AddAWSService<IAmazonSimpleNotificationService>();
            _services.AddSingleton<INotificationService>(sp =>
                new NotificationService(sp.GetRequiredService<IAmazonSimpleNotificationService>(),
                    _configuration.GetValue<string>("NotificationTopicArn")!));

            var connectionString = _configuration.GetConnectionString("Integration");
            var utcHourToRunWithin = _configuration.GetValue<int>("SnapshotReproducerUtcHour");

            _services.AddHostedService<AddressSnapshotReproducer>(provider =>
            {
                var producerOptions = CreateProducerOptions();
                return new AddressSnapshotReproducer(
                    connectionString!,
                    provider.GetRequiredService<IOsloProxy>(),
                    new Producer(producerOptions),
                    provider.GetRequiredService<IClock>(),
                    provider.GetRequiredService<INotificationService>(),
                    utcHourToRunWithin,
                    _loggerFactory);
            });
        }

        private ProducerOptions CreateProducerOptions()
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];
            var topic = $"{_configuration[ProducerProjections.TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerProjections.TopicKey}");
            var producerOptions = new ProducerOptions(
                    new BootstrapServers(bootstrapServers),
                    new Topic(topic),
                    true,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                .ConfigureEnableIdempotence();
            if (!string.IsNullOrEmpty(_configuration["Kafka:SaslUserName"])
                && !string.IsNullOrEmpty(_configuration["Kafka:SaslPassword"]))
            {
                producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                    _configuration["Kafka:SaslUserName"],
                    _configuration["Kafka:SaslPassword"]));
            }

            return producerOptions;
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string producerConnectionString)
        {
            services
                .AddDbContext<ProducerContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(producerConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ProducerSnapshotOslo, Schema.ProducerSnapshotOslo);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ProducerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ProducerContext));
        }
    }
}
