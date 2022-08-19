namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasInvalidGeometryMethodException : AddressRegistryException
    {
        public AddressHasInvalidGeometryMethodException()
        { }

        private AddressHasInvalidGeometryMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
