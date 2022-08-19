namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasInvalidGeometrySpecificationException : AddressRegistryException
    {
        public AddressHasInvalidGeometrySpecificationException()
        { }

        private AddressHasInvalidGeometrySpecificationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
