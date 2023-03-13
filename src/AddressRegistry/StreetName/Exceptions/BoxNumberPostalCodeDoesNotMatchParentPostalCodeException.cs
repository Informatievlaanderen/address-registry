namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException : AddressRegistryException
    {
        public BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        { }

        private BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
