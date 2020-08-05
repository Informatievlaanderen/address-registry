namespace AddressRegistry.Api.Backoffice.Infrastructure
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Microsoft.AspNetCore.Mvc;
    using TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api;

    public class EditApiController : ApiController
    {
        public IActionResult CreatedWithPosition(string location, long position)
            => new CreatedResultWithLastObservedPosition(location, position);

        public IActionResult AcceptedWithPosition(long position)
            => new AcceptedResultWithLastObservedPosition(position);

        protected IDictionary<string, object> GetMetadata()
        {
            // TODO: Move to generic part as well? This is useful!
            if (User == null)
                return new Dictionary<string, object>();

            return new CommandMetadata(
                    User,
                    AddRemoteIpAddressMiddleware.UrnBasisregistersVlaanderenIp,
                    AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)
                .ToDictionary();
        }
    }
}
