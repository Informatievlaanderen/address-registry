namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressCannotBeDeregulatedException : AddressRegistryException
    {
        public AddressCannotBeDeregulatedException(AddressStatus status)
            : base($"Address status '{status}' is invalid for deregulation.")
        { }

        private AddressCannotBeDeregulatedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
