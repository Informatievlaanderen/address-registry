namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class SourceAndDestinationAddressAreTheSameException : AddressRegistryException
    {
        public AddressPersistentLocalId SourceAddressPersistentLocalId { get; }
        public HouseNumber DestinationHouseNumber { get; }

        public SourceAndDestinationAddressAreTheSameException()
        { }

        public SourceAndDestinationAddressAreTheSameException(
            AddressPersistentLocalId sourceAddressPersistentLocalId, HouseNumber destinationHouseNumber)
            : base()
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationHouseNumber = destinationHouseNumber;
        }

        private SourceAndDestinationAddressAreTheSameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
