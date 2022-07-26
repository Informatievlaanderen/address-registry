namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameNotActiveException : AddressRegistryException
    {
        public StreetNameNotActiveException(int streetNameId)
            : base($"StreetName with Id '{streetNameId}' is not active.")
        { }

        private StreetNameNotActiveException(SerializationInfo info, StreamingContext context)
        { }
    }
}
