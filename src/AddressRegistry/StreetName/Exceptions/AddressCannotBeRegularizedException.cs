namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressCannotBeRegularizedException : AddressRegistryException
    {
        public AddressCannotBeRegularizedException(AddressStatus status)
            : base($"Address status '{status}' is invalid for regularization.")
        { }

        private AddressCannotBeRegularizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
