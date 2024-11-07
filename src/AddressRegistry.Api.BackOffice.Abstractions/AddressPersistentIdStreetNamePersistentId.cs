namespace AddressRegistry.Api.BackOffice.Abstractions
{
    public sealed class AddressPersistentIdStreetNamePersistentId
    {
        public int AddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }

        private AddressPersistentIdStreetNamePersistentId()
        { }

        public AddressPersistentIdStreetNamePersistentId(int addressPersistentLocalId, int streetNamePersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
        }
    }
}
