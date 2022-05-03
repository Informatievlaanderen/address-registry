namespace AddressRegistry.Consumer.Read.Municipality.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
            builder
                .RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterProjectionMigrator<ConsumerContextFactory>(
                    _configuration,
                    _loggerFactory)

                //.RegisterProjections<StreetNameConsumerProjection, ConsumerContext>(
                //    context => new StreetNameConsumerProjection(),
                //    ConnectedProjectionSettings.Default)
                ;

            builder.Populate(_services);
        }
    }
}
