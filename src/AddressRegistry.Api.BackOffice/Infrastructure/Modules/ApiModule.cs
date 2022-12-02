namespace AddressRegistry.Api.BackOffice.Infrastructure.Modules
{
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Consumer;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Handlers.Sqs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Syndication;
    using StreetName;
    using Validators;

    public class ApiModule : Module
    {
        internal const string SqsQueueUrlConfigKey = "SqsQueueUrl";
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
            builder
                .RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder
                .RegisterType<StreetNameExistsValidator>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterModule(new IdempotencyModule(
                _services,
                _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                    .ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                _loggerFactory));

            builder.RegisterModule(new EnvelopeModule());
            builder.RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new EditModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new ConsumerModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new MediatRModule());
            builder.RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]));
            builder.RegisterModule(new TicketingModule(_configuration, _services));
            builder.RegisterModule(new MunicipalityConsumerModule(_configuration,_services, _loggerFactory));
            builder.RegisterSnapshotModule(_configuration);

            builder.Populate(_services);
        }
    }
}
