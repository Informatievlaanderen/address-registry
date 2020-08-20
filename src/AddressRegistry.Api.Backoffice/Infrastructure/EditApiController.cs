namespace AddressRegistry.Api.Backoffice.Infrastructure
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.LastObservedPosition;
    using Be.Vlaanderen.Basisregisters.Api.LastObservedPosition.ActionResults;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;

    public class EditApiController : ApiController
    {
        public IActionResult CreatedWithPosition(
            string location,
            long position,
            Instant executionTime)
            => new Created(location, new LastObservedPositionWithCrabDependency(position, executionTime));

        public IActionResult AcceptedWithPosition(
            long position,
            Instant executionTime)
            => new Accepted(new LastObservedPositionWithCrabDependency(position, executionTime));

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
