namespace AddressRegistry.Api.Legacy.Infrastructure.Modules
{
    using AddressMatch.Matching;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Legacy;
    using Projections.Syndication;

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
                .RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory));

            containerBuilder
                .RegisterAssemblyTypes(typeof(IKadRrService).Assembly)
                .AsImplementedInterfaces();

            containerBuilder.Populate(_services);
        }
    }
}
