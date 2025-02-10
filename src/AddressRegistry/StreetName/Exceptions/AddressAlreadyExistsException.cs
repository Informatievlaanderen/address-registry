namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressAlreadyExistsException : AddressRegistryException
    {
        public HouseNumber? HouseNumber { get; }
        public BoxNumber? BoxNumber { get; }

        public AddressAlreadyExistsException()
        { }

        public AddressAlreadyExistsException(HouseNumber houseNumber, BoxNumber? boxNumber)
            : base($"Address with housenumber '{houseNumber}' and boxnumber '{boxNumber}' already exists.")
        {
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
        }

        private AddressAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
