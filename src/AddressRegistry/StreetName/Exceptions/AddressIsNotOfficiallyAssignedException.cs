namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressIsNotOfficiallyAssignedException : AddressRegistryException
    {
        public AddressIsNotOfficiallyAssignedException()
        { }

        private AddressIsNotOfficiallyAssignedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
