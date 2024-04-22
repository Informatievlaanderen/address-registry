namespace AddressRegistry.Api.Extract.Extracts
{
    using System;

    internal static class ExtractFileNames
    {
        public const string Address = "Adres";
        public const string BuildingUnitLinks = "AdresGebouweenheidKoppelingen";
        public const string ParcelLinks = "AdresPerceelKoppelingen";

        public static string GetAddressZip() => $"Adres-{DateTime.Today:yyyy-MM-dd}";
        public static string GetAddressLinksZip() => $"Adreskoppelingen-{DateTime.Today:yyyy-MM-dd}";
    }
}
