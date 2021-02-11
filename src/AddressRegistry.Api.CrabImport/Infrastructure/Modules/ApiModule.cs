namespace AddressRegistry.Api.CrabImport.Infrastructure.Modules
{
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CrabImport;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using CrabImport;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Legacy;

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

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule(new LegacyModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new EnvelopeModule())
                .RegisterModule(new EditModule(_configuration, _services, _loggerFactory))

                .RegisterModule(
                    new CrabImportModule(
                        _services,
                        _configuration.GetConnectionString("CrabImport"),
                        Schema.Import,
                        _loggerFactory))

                .RegisterModule(new IdempotencyModule(
                    _services,
                    _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>().ConnectionString,
                    new IdempotencyMigrationsTableInfo(Schema.Import),
                    new IdempotencyTableInfo(Schema.Import),
                    _loggerFactory));

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModule>()
                .AsSelf();

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModuleProcessor>()
                .As<IIdempotentCommandHandlerModuleProcessor>();
            
            containerBuilder
                .RegisterType<IdempotentCommandHandlerModule>()
                .AsSelf();

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModuleProcessor>()
                .As<IIdempotentCommandHandlerModuleProcessor>();

            containerBuilder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            containerBuilder.Populate(_services);
        }
    }
}
