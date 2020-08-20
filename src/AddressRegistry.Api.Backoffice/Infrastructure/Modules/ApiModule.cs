namespace AddressRegistry.Api.Backoffice.Infrastructure.Modules
{
    using Address;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Crab.Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using CrabEdit.Client;
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

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule(new CrabEditModule(_configuration))
                .RegisterModule(new EditModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new CrabMappingModule());

            containerBuilder
                .RegisterType<AddressCrabEditClient>();

            containerBuilder.Populate(_services);
        }
    }
}
