namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameAddressChildAlreadyExistsException : AddressRegistryException
    {
        public StreetNameAddressChildAlreadyExistsException()
        { }

        private StreetNameAddressChildAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
