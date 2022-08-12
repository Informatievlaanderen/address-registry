namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DuplicateBoxNumberException : AddressRegistryException
    {
        public DuplicateBoxNumberException(BoxNumber boxNumber)
            : base($"Attempt to add child address with duplicate boxnumber '{boxNumber}'.")
        { }

        private DuplicateBoxNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
