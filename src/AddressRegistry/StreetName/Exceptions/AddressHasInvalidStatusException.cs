namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasInvalidStatusException : AddressRegistryException
    {
        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public AddressHasInvalidStatusException()
        { }

        public AddressHasInvalidStatusException(AddressPersistentLocalId addressPersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        private AddressHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
