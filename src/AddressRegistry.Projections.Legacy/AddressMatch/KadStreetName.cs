namespace AddressRegistry.Projections.Legacy.AddressMatch
{
    public class KadStreetName
    {
        public const string TableName = "KadStreetNames";

        public int StreetNameId { get; set; }
        public string KadStreetNameCode { get; set; }
        public string NisCode { get; set; }
    }
}
