namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class HouseNumberToCorrectHasBoxNumberException : AddressRegistryException
    {
        public HouseNumberToCorrectHasBoxNumberException()
        { }

        private HouseNumberToCorrectHasBoxNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public HouseNumberToCorrectHasBoxNumberException(string message)
            : base(message)
        { }

        public HouseNumberToCorrectHasBoxNumberException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
