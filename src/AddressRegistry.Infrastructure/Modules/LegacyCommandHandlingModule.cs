namespace AddressRegistry.Infrastructure.Modules
{
    using Address;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Microsoft.Extensions.Configuration;

    public class LegacyCommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public LegacyCommandHandlingModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterModule(new AggregateSourceModule(_configuration));

            containerBuilder
                .RegisterType<CrabAddressProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<AddressLegacyProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<AddressPersistentLocalIdentifierProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<AddressProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<CrabAddressCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(CrabAddressCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
