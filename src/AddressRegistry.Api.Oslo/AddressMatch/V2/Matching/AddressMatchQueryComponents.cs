namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    public sealed class AddressMatchQueryComponents
    {
        public string MunicipalityName { get; set; }
        public string NisCode { get; set; }
        public string PostalCode { get; set; }
        public string StreetName { get; set; }
        public string HouseNumber { get; set; }
        public string BoxNumber { get; set; }
    }
}
