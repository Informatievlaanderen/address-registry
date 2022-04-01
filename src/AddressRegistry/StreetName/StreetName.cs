namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Events;

    public partial class StreetName : AggregateRootEntity
    {
        public static readonly Func<StreetName> Factory = () => new StreetName();

        public static StreetName Register(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            StreetNameStatus streetNameStatus)
        {
            var streetName = Factory();
            streetName.ApplyChange(new StreetNameWasImported(streetNamePersistentLocalId, municipalityId, streetNameStatus));
            return streetName;
        }
    }
}
