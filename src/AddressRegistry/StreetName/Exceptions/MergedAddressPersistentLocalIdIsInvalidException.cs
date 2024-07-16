namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class MergedAddressPersistentLocalIdIsInvalidException : AddressRegistryException
    {
        public MergedAddressPersistentLocalIdIsInvalidException()
        { }

        private MergedAddressPersistentLocalIdIsInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
