namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressBoxNumberHasInconsistentHouseNumberException : AddressRegistryException
    {
        public AddressBoxNumberHasInconsistentHouseNumberException()
        { }

        private AddressBoxNumberHasInconsistentHouseNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
