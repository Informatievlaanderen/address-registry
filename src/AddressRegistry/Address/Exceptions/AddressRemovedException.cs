namespace AddressRegistry.Address.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressRemovedException : AddressRegistryException
    {
        public AddressRemovedException()
        { }

        private AddressRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
        
        public AddressRemovedException(string message)
            : base(message)
        { }

        public AddressRemovedException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
