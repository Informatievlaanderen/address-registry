namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressNotFoundException : AddressRegistryException
    {
        public AddressNotFoundException(AddressPersistentLocalId addressPersistentLocalId)
            : base($"Address with Id '{addressPersistentLocalId}' has not been found.")
        { }

        private AddressNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
