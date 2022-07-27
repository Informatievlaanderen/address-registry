namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressCannotBeRejectedException : AddressRegistryException
    {
        public AddressCannotBeRejectedException(AddressStatus status)
            : base($"Address status '{status}' is invalid for rejection.")
        { }

        private AddressCannotBeRejectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
