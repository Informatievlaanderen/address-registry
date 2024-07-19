namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class MunicipalityMergerAddressIsNotFoundException : AddressRegistryException
    {
        public MunicipalityMergerAddressIsNotFoundException()
        { }

        public MunicipalityMergerAddressIsNotFoundException(AddressPersistentLocalId addressPersistentLocalId)
            : base($"No new address was found for address with Id '{addressPersistentLocalId}' has not been found.")
        { }

        private MunicipalityMergerAddressIsNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
