namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressPersistentLocalIdAlreadyExistsException : AddressRegistryException
    {
        public AddressPersistentLocalIdAlreadyExistsException()
        { }

        private AddressPersistentLocalIdAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
