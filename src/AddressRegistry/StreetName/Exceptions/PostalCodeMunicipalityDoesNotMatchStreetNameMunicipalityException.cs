namespace AddressRegistry.StreetName.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException : AddressRegistryException
    {
        public PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException()
        { }
        
        public PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException(string message)
            : base(message)
        { }

        private PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
