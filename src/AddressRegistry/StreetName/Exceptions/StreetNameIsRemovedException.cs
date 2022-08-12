namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameIsRemovedException : AddressRegistryException
    {
        public StreetNameIsRemovedException(StreetNamePersistentLocalId streetNameId)
            : base($"StreetName with Id '{streetNameId}' is removed.")
        { }

        private StreetNameIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
