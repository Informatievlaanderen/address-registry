namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasNoPostalCodeException : AddressRegistryException
    {
        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public AddressHasNoPostalCodeException()
        { }

        public AddressHasNoPostalCodeException(AddressPersistentLocalId addressPersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        private AddressHasNoPostalCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public AddressHasNoPostalCodeException(string message)
            : base(message)
        { }

        public AddressHasNoPostalCodeException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
