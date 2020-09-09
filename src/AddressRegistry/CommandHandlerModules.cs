namespace AddressRegistry
{
    using Address;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<CrabAddressProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<AddressPersistentLocalIdentifierProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<AddressProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<AddressCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(AddressCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
