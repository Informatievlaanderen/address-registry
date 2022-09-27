namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasNoBoxNumberException : AddressRegistryException
    {
        public AddressHasNoBoxNumberException()
        { }

        private AddressHasNoBoxNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public AddressHasNoBoxNumberException(string message)
            : base(message)
        { }

        public AddressHasNoBoxNumberException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
