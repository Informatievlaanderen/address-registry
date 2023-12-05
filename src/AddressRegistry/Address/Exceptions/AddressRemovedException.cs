namespace AddressRegistry.Address.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Obsolete("This is a legacy class and should not be used anymore.")]
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
