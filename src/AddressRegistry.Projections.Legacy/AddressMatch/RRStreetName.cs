namespace AddressRegistry.Projections.Legacy.AddressMatch
{
    public class RRStreetName
    {
        public const string TableName = "RRStreetNames";

        public int StreetNameId { get; set; }
        public string StreetName { get; set; }
        public string StreetCode { get; set; }
        public string PostalCode { get; set; }
    }
}
