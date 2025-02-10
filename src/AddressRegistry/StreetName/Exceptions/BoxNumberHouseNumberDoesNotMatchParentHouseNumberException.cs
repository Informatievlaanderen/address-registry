namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BoxNumberHouseNumberDoesNotMatchParentHouseNumberException : AddressRegistryException
    {
        public BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        { }

        private BoxNumberHouseNumberDoesNotMatchParentHouseNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
