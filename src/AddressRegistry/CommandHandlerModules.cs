namespace AddressRegistry
{
    using Address;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using StreetName;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
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

            containerBuilder
                .RegisterType<StreetNameProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<StreetNameCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(StreetNameCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();

            containerBuilder
                .RegisterType<AddressCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(AddressCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
