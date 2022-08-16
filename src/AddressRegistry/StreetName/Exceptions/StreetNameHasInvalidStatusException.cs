namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameHasInvalidStatusException : AddressRegistryException
    {
        public StreetNameHasInvalidStatusException()
        { }

        private StreetNameHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
