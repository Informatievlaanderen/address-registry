namespace AddressRegistry.Api.Projector.Projections
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Controllers;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("1.0")]
    [ApiRoute("projections")]
    public class ProjectionsController : DefaultProjectorController
    {
        public ProjectionsController(IConnectedProjectionsManager connectedProjectionsManager)
            : base(connectedProjectionsManager)
        { }
    }
}
