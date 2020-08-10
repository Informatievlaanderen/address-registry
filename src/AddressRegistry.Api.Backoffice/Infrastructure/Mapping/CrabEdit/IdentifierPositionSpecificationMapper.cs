namespace AddressRegistry.Api.Backoffice.Infrastructure.Mapping.CrabEdit
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Mappers;
    using global::CrabEdit.Infrastructure.Address;

    public class IdentifierPositionSpecificationMapper : DefaultIdentifierUriMapper<AddressPositionSpecification>
    {
        protected override IDictionary<IdentifierUri, AddressPositionSpecification> Mapping
            => new Dictionary<IdentifierUri, AddressPositionSpecification>
            {
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/ligplaats"), AddressPositionSpecification.Berth },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/gebouweenheid"), AddressPositionSpecification.BuildingUnit },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/ingang"), AddressPositionSpecification.Entry },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/lot"), AddressPositionSpecification.Lot },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/perceel"), AddressPositionSpecification.Parcel },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/standplaats"), AddressPositionSpecification.Stand }
            };
    }
}
