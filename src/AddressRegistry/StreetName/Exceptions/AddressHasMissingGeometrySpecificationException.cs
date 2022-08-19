namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasMissingGeometrySpecificationException : AddressRegistryException
    {
        public AddressHasMissingGeometrySpecificationException()
        { }

        private AddressHasMissingGeometrySpecificationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
