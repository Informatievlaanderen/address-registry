namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BoxNumberHasInvalidFormatException : AddressRegistryException
    {
        public BoxNumberHasInvalidFormatException()
        { }

        private BoxNumberHasInvalidFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public BoxNumberHasInvalidFormatException(string message)
            : base(message)
        { }

        public BoxNumberHasInvalidFormatException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
