namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasBoxNumberException : AddressRegistryException
    {
        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public AddressHasBoxNumberException()
        { }

        public AddressHasBoxNumberException(AddressPersistentLocalId addressPersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        private AddressHasBoxNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public AddressHasBoxNumberException(string message)
            : base(message)
        { }

        public AddressHasBoxNumberException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
