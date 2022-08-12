namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ParentAddressNotFoundException : AddressRegistryException
    {
        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public HouseNumber HouseNumber { get; }

        public ParentAddressNotFoundException(StreetNamePersistentLocalId streetNamePersistentLocalId, HouseNumber houseNumber)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            HouseNumber = houseNumber;
        }

        private ParentAddressNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
