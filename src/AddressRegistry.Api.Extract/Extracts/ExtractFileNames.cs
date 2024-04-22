namespace AddressRegistry.Api.Extract.Extracts
{
    using System;

    internal static class ExtractFileNames
    {
        public const string Address = "Adres";
        public const string BuildingUnitLinks = "AdresGebouweenheidKoppelingen";
        public const string ParcelLinks = "AdresPerceelKoppelingen";
        public const string PostalCodeStreetNameLinks = "PostcodeStraatnaamKoppelingen";

        public static string GetAddressZip() => $"Adres-{DateTime.Today:yyyy-MM-dd}";
        public static string GetAddressLinksZip() => $"Adreskoppelingen-{DateTime.Today:yyyy-MM-dd}";
        public static string GetPostalCodeStreetNameLinksZip() => $"{PostalCodeStreetNameLinks}-{DateTime.Today:yyyy-MM-dd}";
    }
}
