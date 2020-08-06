namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Crab.Modules
{
    using Autofac;
    using GeoJsonMapping;

    public class CrabMappingModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(typeof(GeoJsonObjectMapper<>).Assembly)
                .AsClosedTypesOf(typeof(GeoJsonObjectMapper<>))
                .As<IGeoJsonObjectMapper>();

            containerBuilder
                .RegisterType<GeoJsonMapper>();
        }
    }
}
