namespace AddressRegistry.Api.BackOffice.Validators
{
    public static class ValidationErrorMessages
    {
        public const string StreetNameInvalid = "Ongeldige straatnaamId.";
        public const string StreetNameIsNotActive = "De straatnaam is gehistoreerd of afgekeurd.";

        public const string AddressAlreadyExists = "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.";
        public static string AddressHouseNumberUnknown(string streetNamePersistentLocalId, string houseNumber)
            => $"Er bestaat geen actief adres zonder busnummer voor straatnaamobject '{streetNamePersistentLocalId}' en huisnummer '{houseNumber}'.";

        public const string PostalCodeDoesNotExist = "Ongeldige postinfoId.";

        public const string HouseNumberInvalid = "Ongeldig huisnummerformaat.";

        public const string BoxNumberInvalid = "Ongeldig busnummerformaat.";
    }
}
