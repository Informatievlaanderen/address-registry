namespace AddressRegistry.Producer.Infrastructure.Modules
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
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
    using SqlStreamStore;

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
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

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
                nameof(ProducerContext), Schema.Producer, MigrationTables.Producer);

            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerMigrateProjections, ProducerContext>(() =>
                {
                    var bootstrapServers = _configuration["Kafka:BootstrapServers"]!;
                    var topic =
                        $"{_configuration[ProducerMigrateProjections.AddressTopicKey]}"
                        ?? throw new ArgumentException($"Configuration has no value for {ProducerMigrateProjections.AddressTopicKey}");
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
                            _configuration["Kafka:SaslUserName"]!,
                            _configuration["Kafka:SaslPassword"]!));
                    }

                    return new ProducerMigrateProjections(new Producer(producerOptions));
                }, connectedProjectionSettings)
                .RegisterProjections<ProducerMigrateReaddressFixProjections, ProducerContext>(c =>
                {
                    var bootstrapServers = _configuration["Kafka:BootstrapServers"]!;
                    var topic =
                        $"{_configuration[ProducerMigrateReaddressFixProjections.AddressTopicKey]}"
                        ?? throw new ArgumentException($"Configuration has no value for {ProducerMigrateReaddressFixProjections.AddressTopicKey}");
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
                            _configuration["Kafka:SaslUserName"]!,
                            _configuration["Kafka:SaslPassword"]!));
                    }

                    return new ProducerMigrateReaddressFixProjections(
                        new Producer(producerOptions),
                        c.Resolve<IReadonlyStreamStore>());
                }, connectedProjectionSettings)
                .RegisterProjections<ProducerProjectionsV3, ProducerContext>(c =>
                {
                    var bootstrapServers = _configuration["Kafka:BootstrapServers"]!;
                    var topic =
                        $"{_configuration[ProducerProjectionsV3.AddressTopicKey]}"
                        ?? throw new ArgumentException($"Configuration has no value for {ProducerProjectionsV3.AddressTopicKey}");
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
                            _configuration["Kafka:SaslUserName"]!,
                            _configuration["Kafka:SaslPassword"]!));
                    }

                    return new ProducerProjectionsV3(
                        new Producer(producerOptions),
                        c.Resolve<IReadonlyStreamStore>());
                }, connectedProjectionSettings);
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
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Producer, Schema.Producer);
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
