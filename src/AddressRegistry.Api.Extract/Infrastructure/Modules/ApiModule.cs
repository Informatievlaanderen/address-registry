namespace AddressRegistry.Api.Extract.Infrastructure.Modules
{
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Consumer.Read.StreetName.Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Extract;
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

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterModule(new ExtractModule(_configuration, _services, _loggerFactory, false))
                .RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MunicipalityConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new StreetNameConsumerModule(_configuration, _services, _loggerFactory));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }
    }
}
