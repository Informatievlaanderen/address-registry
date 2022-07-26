namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ParentAddressNotFoundException : AddressRegistryException
    {
        public readonly string? StreetNamePersistentLocalId;
        public readonly string? HouseNumber;

        public ParentAddressNotFoundException(string streetNamePersistentLocalId, string houseNumber)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            HouseNumber = houseNumber;
        }

        private ParentAddressNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
