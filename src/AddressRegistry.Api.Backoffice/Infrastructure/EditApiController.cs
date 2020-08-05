namespace AddressRegistry.Api.Backoffice.Infrastructure
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;

    public class EditApiController : ApiController
    {
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
