namespace AddressRegistry.Api.Oslo.Address.List
{
    public class AddressFilter
    {
        public string BoxNumber { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public string MunicipalityName { get; set; }
        public string StreetName { get; set; }
        public string HomonymAddition { get; set; }
        public string Status { get; set; }
        public string? NisCode { get; set; }
        public string? StreetNameId { get; set; }
    }
}
