namespace AddressRegistry.Api.Backoffice.Infrastructure
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api;

    public class EditApiController : ApiController
    {
        public IActionResult CreatedWithPosition(
            string location,
            long position,
            Instant executionTime)
            => new CreatedResultWithLastObservedPosition(location, new CrabLastObservedPosition(position, executionTime));

        public IActionResult AcceptedWithPosition(
            long position,
            Instant executionTime)
            => new AcceptedResultWithLastObservedPosition(new CrabLastObservedPosition(position, executionTime));

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
