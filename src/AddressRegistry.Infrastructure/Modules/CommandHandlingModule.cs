namespace AddressRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Microsoft.Extensions.Configuration;
    using StreetName;

    public class CommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterModule(new AggregateSourceModule(_configuration));

            containerBuilder
                .RegisterType<StreetNameProvenanceFactory>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder
                .RegisterType<StreetNameCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(StreetNameCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();

            containerBuilder
                .RegisterType<AddressCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(AddressCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();

            containerBuilder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
