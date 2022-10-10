namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressAlreadyExistsException : AddressRegistryException
    {
        public AddressAlreadyExistsException()
        { }

        public AddressAlreadyExistsException(HouseNumber houseNumber, BoxNumber boxNumber)
            : base($"Address with housenumber '{houseNumber}' and boxnumber '{boxNumber}' already exists.")
        { }

        private AddressAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
