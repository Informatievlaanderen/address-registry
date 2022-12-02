namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressBoxNumberHasInconsistentPostalCodeException : AddressRegistryException
    {
        public AddressBoxNumberHasInconsistentPostalCodeException()
        { }

        private AddressBoxNumberHasInconsistentPostalCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
