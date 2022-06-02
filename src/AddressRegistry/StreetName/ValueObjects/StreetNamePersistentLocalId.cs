namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class StreetNamePersistentLocalId : IntegerValueObject<StreetNamePersistentLocalId>
    {
        public StreetNamePersistentLocalId(int streetNamePersistentLocalId)
            : base(streetNamePersistentLocalId)
        { }
    }
}
