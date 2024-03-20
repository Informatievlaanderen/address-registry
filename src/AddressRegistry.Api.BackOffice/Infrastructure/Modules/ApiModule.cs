namespace AddressRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Abstractions.SqsRequests;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance.AcmIdm;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Syndication;
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
            builder.RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder.Register(c => new AcmIdmProvenanceFactory(Application.AddressRegistry, c.Resolve<IActionContextAccessor>()))
                .As<IProvenanceFactory>()
                .InstancePerLifetimeScope()
                .AsSelf();

            builder.RegisterType<HouseNumberValidator>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<StreetNameExistsValidator>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<ProposeAddressSqsRequestFactory>()
                .AsSelf();

            _services.ConfigureIdempotency(
                _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                    .ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                _loggerFactory);

            builder.RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new AggregateSourceModule(_configuration));
            builder.RegisterModule(new SequenceModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new MediatRModule());
            builder.RegisterModule(new FluentValidationModule());
            builder.RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]));
            builder.RegisterModule(new TicketingModule(_configuration, _services));

            builder.RegisterSnapshotModule(_configuration);

            _services.AddAcmIdmAuthorizationHandlers();

            builder.Populate(_services);
        }
    }
}
