namespace AddressRegistry.Api.Extract.Extracts
{
    using System;

    internal static class ExtractFileNames
    {
        public const string Address = "Adres";
        public const string CrabHouseNumberId = "CrabHuisnummer";
        public const string CrabSubadresId = "CrabSubadres";
        public const string BuildingUnitLinks = "Adreskoppelingen";
        public const string ParcelLinks = "Adreskoppelingen_1";

        public static string GetAddressZip() => $"Adres-{DateTime.Today:yyyy-MM-dd}";
        public static string GetAddressLinksZip() => $"Adreskoppelingen-{DateTime.Today:yyyy-MM-dd}";
    }
}
