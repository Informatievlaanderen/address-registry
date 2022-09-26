namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BoxNumberAlreadyExistsException : AddressRegistryException
    {
        public BoxNumberAlreadyExistsException()
        { }

        public BoxNumberAlreadyExistsException(BoxNumber boxNumber)
            : base($"Attempt to add child address with duplicate boxnumber '{boxNumber}'.")
        { }

        private BoxNumberAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
