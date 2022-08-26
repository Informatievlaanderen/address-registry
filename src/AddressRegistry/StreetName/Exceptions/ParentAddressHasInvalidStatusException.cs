namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ParentAddressHasInvalidStatusException : AddressRegistryException
    {
        public ParentAddressHasInvalidStatusException()
        { }

        private ParentAddressHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
