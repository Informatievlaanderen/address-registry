namespace AddressRegistry.Projections.LastChangedList.Modules
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Autofac;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;

    public class ProjectionsModule : Module
    {
        private readonly IConfiguration _configuration;

        public ProjectionsModule(IConfiguration configuration) => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));

            builder.RegisterModule<EnvelopeModule>();

            builder.RegisterEventstreamModule(_configuration);
        }
    }
}
