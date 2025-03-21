namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressIsNotFoundException : AddressRegistryException
    {
        public AddressPersistentLocalId? AddressPersistentLocalId { get; }

        public AddressIsNotFoundException()
        { }

        public AddressIsNotFoundException(AddressPersistentLocalId addressPersistentLocalId)
            : base($"Address with Id '{addressPersistentLocalId}' has not been found.")
        {
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        private AddressIsNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
