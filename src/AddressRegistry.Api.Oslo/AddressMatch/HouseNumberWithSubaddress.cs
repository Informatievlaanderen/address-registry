namespace AddressRegistry.Api.Oslo.AddressMatch
{
    public sealed class HouseNumberWithSubaddress
    {
        public string HouseNumber { get; }
        public string? BoxNumber { get; }
        public string? AppNumber { get; }

        internal HouseNumberWithSubaddress(
            string houseNumber,
            string? boxNumber,
            string? appNumber)
        {
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            AppNumber = appNumber;
        }
    }
}
