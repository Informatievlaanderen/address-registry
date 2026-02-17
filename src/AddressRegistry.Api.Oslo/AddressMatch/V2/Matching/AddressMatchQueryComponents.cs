namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using StreetName;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public sealed class AddressMatchQueryComponents
    {
        public string MunicipalityName { get; set; }
        public string NisCode { get; set; }
        public string PostalCode { get; set; }
        public string StreetName { get; set; }
        public string HouseNumber { get; set; }
        public string BoxNumber { get; set; }
        public AddressStatus? AddressStatus { get; set; }
        public StreetNameStatus? StreetNameStatus { get; set; }
    }
}
