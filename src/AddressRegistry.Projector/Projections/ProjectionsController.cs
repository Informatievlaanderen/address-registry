namespace AddressRegistry.Projector.Projections
{
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("projections")]
    public class ProjectionsController : DefaultProjectorController
    {
        public ProjectionsController(
            IConnectedProjectionsManager connectedProjectionsManager,
            IConfiguration configuration)
            : base(
                connectedProjectionsManager,
                configuration.GetValue<string>("BaseUrl"))
        {
            RegisterConnectionString(Schema.Legacy, configuration.GetConnectionString("LegacyProjections"));
            RegisterConnectionString(Schema.Extract, configuration.GetConnectionString("ExtractProjections"));
            RegisterConnectionString(Schema.Wfs, configuration.GetConnectionString("WfsProjections"));
            RegisterConnectionString(Schema.Wms, configuration.GetConnectionString("WmsProjections"));

            RegisterConnectionString(Schema.Integration, configuration.GetConnectionString("IntegrationProjections"));
        }
    }
}
