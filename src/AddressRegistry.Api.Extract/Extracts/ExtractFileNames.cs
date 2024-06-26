namespace AddressRegistry.Api.Extract.Extracts
{
    using System;

    internal static class ExtractFileNames
    {
        public const string Address = "Adres";
        public const string PostalCodeStreetNameLinks = "PostcodeStraatnaamKoppelingen";
        public const string CrabHouseNumberId = "CrabHuisnummer";
        public const string CrabSubadresId = "CrabSubadres";

        public static string GetAddressZip() => $"{Address}-{DateTime.Today:yyyy-MM-dd}";
        public static string GetPostalCodeStreetNameLinksZip() => $"{PostalCodeStreetNameLinks}-{DateTime.Today:yyyy-MM-dd}";
        public static string GetAddressCrabZip() => $"Adres-Crab-{DateTime.Today:yyyy-MM-dd}";
    }
}
