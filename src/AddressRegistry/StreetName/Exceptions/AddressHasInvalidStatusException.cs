namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasInvalidStatusException : AddressRegistryException
    {
        public AddressHasInvalidStatusException()
        { }

        private AddressHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
