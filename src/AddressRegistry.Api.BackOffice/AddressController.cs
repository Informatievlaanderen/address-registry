namespace AddressRegistry.Api.BackOffice
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "adressen")]
    public partial class AddressController : ApiController
    {
        private readonly IMediator _mediator;
        public AddressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private ValidationException CreateValidationException(string errorCode, string propertyName, string message)
        {
            var failure = new ValidationFailure(propertyName, message)
            {
                ErrorCode = errorCode
            };

            return new ValidationException(new List<ValidationFailure>
            {
                failure
            });
        }

        private async Task<string> GetHash(
            IStreetNames streetnames,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await streetnames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), cancellationToken);
            var streetNameHash = aggregate.GetAddressHash(addressPersistentLocalId);
            return streetNameHash;
        }

        protected IDictionary<string, object?> GetMetadata()
        {
            var userId = User.FindFirst("urn:be:vlaanderen:addressregistry:acmid")?.Value;
            var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

            return new Dictionary<string, object?>
            {
                { "UserId", userId },
                { "CorrelationId", correlationId }
            };
        }
    }
}