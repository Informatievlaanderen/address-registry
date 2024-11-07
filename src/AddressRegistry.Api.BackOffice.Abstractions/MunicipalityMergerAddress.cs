namespace AddressRegistry.Api.BackOffice.Abstractions
{
    public sealed class MunicipalityMergerAddress
    {
        public int OldStreetNamePersistentLocalId { get; init; }
        public int OldAddressPersistentLocalId { get; init; }
        public int NewStreetNamePersistentLocalId { get; init; }
        public int NewAddressPersistentLocalId { get; init; }

        private MunicipalityMergerAddress()
        {
        }

        public MunicipalityMergerAddress(
            int oldStreetNamePersistentLocalId,
            int oldAddressPersistentLocalId,
            int newStreetNamePersistentLocalId,
            int newAddressPersistentLocalId)
        {
            OldStreetNamePersistentLocalId = oldStreetNamePersistentLocalId;
            OldAddressPersistentLocalId = oldAddressPersistentLocalId;
            NewStreetNamePersistentLocalId = newStreetNamePersistentLocalId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
        }
    }
}
