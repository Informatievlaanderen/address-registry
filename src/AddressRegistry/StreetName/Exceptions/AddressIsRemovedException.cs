namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressIsRemovedException : AddressRegistryException
    {
        public AddressIsRemovedException()
        { }

        public AddressIsRemovedException(AddressPersistentLocalId addressPersistentLocalId)
            : base($"Address with Id '{addressPersistentLocalId}' is removed.")
        { }

        private AddressIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
