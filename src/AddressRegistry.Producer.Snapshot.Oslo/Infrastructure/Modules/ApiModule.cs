namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure.Modules
{
    using System;
    using System.Net.Http;
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
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
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
            builder
                .RegisterModule(
                    new ProducerModule(
                        _configuration,
                        _services,
                        _loggerFactory));

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

            _services.AddHostedService<SnapshotReproducer>(provider =>
            {
                var producerOptions = CreateProducerOptions();
                return new SnapshotReproducer(
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
    }
}
