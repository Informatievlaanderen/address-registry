namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressCannotBeApprovedException : AddressRegistryException
    {
        public AddressCannotBeApprovedException(AddressStatus status)
            : base($"Address status '{status}' is invalid for approval.")
        { }

        private AddressCannotBeApprovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
