namespace AddressRegistry.Api.Backoffice.Infrastructure.Mapping.CrabEdit
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Mappers;
    using global::CrabEdit.Infrastructure.Address;

    public class IdentifierAddressStatusMapper : DefaultIdentifierUriMapper<AddressStatus>
    {
        protected override IDictionary<IdentifierUri, AddressStatus> Mapping
            => new Dictionary<IdentifierUri, AddressStatus>
            {
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/adresstatus/voorgesteld"), AddressStatus.Proposed },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/adresstatus/inGebruik"), AddressStatus.InUse },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/adresstatus/gehistoreerd"), AddressStatus.Retired }
            };
    }
}
