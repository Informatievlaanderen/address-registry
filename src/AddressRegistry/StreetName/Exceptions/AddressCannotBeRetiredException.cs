namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressCannotBeRetiredException : AddressRegistryException
    {
        public AddressCannotBeRetiredException(AddressStatus status)
            : base($"Address status '{status}' is invalid for retiring.")
        { }

        private AddressCannotBeRetiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
