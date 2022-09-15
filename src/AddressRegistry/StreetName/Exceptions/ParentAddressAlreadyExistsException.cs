namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ParentAddressAlreadyExistsException : AddressRegistryException
    {
        public ParentAddressAlreadyExistsException()
        { }

        public ParentAddressAlreadyExistsException(HouseNumber houseNumber)
            : base($"Attempt to add parent address when parent address with housenumber '{houseNumber}' already exists for street.")
        { }

        private ParentAddressAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
