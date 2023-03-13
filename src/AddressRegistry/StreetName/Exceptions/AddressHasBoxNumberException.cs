namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasBoxNumberException : AddressRegistryException
    {
        public AddressHasBoxNumberException()
        { }

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
