namespace AddressRegistry.StreetName.DataStructures
{
    public class AddressBoxNumberReplacedBecauseOfReaddressData
    {
        public int SourceAddressPersistentLocalId { get; }
        public int DestinationAddressPersistentLocalId { get; }

        public AddressBoxNumberReplacedBecauseOfReaddressData(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
        }
    }
}