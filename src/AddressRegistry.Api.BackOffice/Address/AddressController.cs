namespace AddressRegistry.Api.BackOffice.Address
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "adressen")]
    public partial class AddressController : ApiBusController
    {
        public AddressController(ICommandHandlerResolver bus) : base(bus) { }

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

        private Provenance CreateFakeProvenance()
        {
            return new Provenance(
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                Application.StreetNameRegistry,
                new Reason(""), // TODO: TBD
                new Operator(""), // TODO: from claims
                Modification.Insert,
                Organisation.DigitaalVlaanderen // TODO: from claims
            );
        }
    }
}
