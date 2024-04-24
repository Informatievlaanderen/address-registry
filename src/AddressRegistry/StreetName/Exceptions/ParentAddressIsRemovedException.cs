namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ParentAddressIsRemovedException : AddressRegistryException
    {
        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public HouseNumber HouseNumber { get; }

        public ParentAddressIsRemovedException(StreetNamePersistentLocalId streetNamePersistentLocalId, HouseNumber houseNumber)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            HouseNumber = houseNumber;
        }

        private ParentAddressIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
