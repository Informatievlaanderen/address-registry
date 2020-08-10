namespace AddressRegistry.Api.Backoffice.Infrastructure.Mapping.CrabEdit
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Mappers;
    using global::CrabEdit.Infrastructure.Address;

    public class IdentifierPositionGeometryMethodMapper : DefaultIdentifierUriMapper<AddressPositionMethod>
    {
        protected override IDictionary<IdentifierUri, AddressPositionMethod> Mapping
            => new Dictionary<IdentifierUri, AddressPositionMethod>
            {
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriemethode/aangeduidDoorBeheerder"), AddressPositionMethod.AppointedByAdministrator },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriemethode/afgeleidVanObject"), AddressPositionMethod.DerivedFromObject }
            };
    }
}
