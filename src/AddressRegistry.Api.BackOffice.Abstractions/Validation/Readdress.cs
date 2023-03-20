namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class Readdress
        {
            public static class EmptyAddressesToReaddress
            {
                public const string Code = "HerAdresseerLijstLeeg";
                public const string Message = "De lijst van te heradresseren adressen kan niet leeg zijn.";
            }

            public static class AddressNotFound
            {
                public const string Code = "AdresIsOnbestaand";
                public static string Message(string addressId) => $"Onbestaand adres '{addressId}'.";
            }

            public static class HouseNumberInvalidFormat
            {
                public const string Code = "AdresOngeldigHuisnummerformaat";
                public static string Message(string houseNumber) => $"Ongeldig huisnummerformaat '{houseNumber}'.";
            }
        }
    }
}
