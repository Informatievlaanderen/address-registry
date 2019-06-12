namespace AddressRegistry.Projections.Legacy.AddressMatch
{
    public class RRAddress
    {
        public const string TableName = "RRAddresses";

        public int AddressId { get; set; }
        public string AddressType { get; set; }
        public string RRHouseNumber { get; set; }
        public string RRIndex { get; set; }
        public string StreetCode { get; set; }
        public string PostalCode { get; set; }
    }
}
