namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasMissingPositionException : AddressRegistryException
    {
        public AddressHasMissingPositionException()
        { }

        private AddressHasMissingPositionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
