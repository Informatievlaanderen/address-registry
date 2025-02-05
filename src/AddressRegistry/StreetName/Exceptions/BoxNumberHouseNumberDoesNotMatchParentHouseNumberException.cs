namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BoxNumberHouseNumberDoesNotMatchParentHouseNumberException : AddressRegistryException
    {
        public AddressPersistentLocalId? BoxNumberAddressPersistentLocalId { get; }
        public AddressPersistentLocalId? NotMatchingHouseNumberAddressPersistentLocalId { get; }

        public BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        { }

        public BoxNumberHouseNumberDoesNotMatchParentHouseNumberException(
            AddressPersistentLocalId boxNumberAddressPersistentLocalId,
            AddressPersistentLocalId notMatchingHouseNumberAddressPersistentLocalId)
        {
            BoxNumberAddressPersistentLocalId = boxNumberAddressPersistentLocalId;
            NotMatchingHouseNumberAddressPersistentLocalId = notMatchingHouseNumberAddressPersistentLocalId;
        }

        private BoxNumberHouseNumberDoesNotMatchParentHouseNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
