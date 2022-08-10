namespace AddressRegistry.Api.BackOffice.Validators
{
    public static class ValidationErrorMessages
    {
        public static string StreetNameInvalid(string streetNamePuri)
            => $"De straatnaam '{streetNamePuri}' is niet gekend in het straatnaamregister.";

        public const string StreetNameIsNotActive = "De straatnaam is gehistoreerd of afgekeurd.";

        public const string AddressAlreadyExists = "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.";

        public static string AddressHouseNumberUnknown(string streetNamePuri, string houseNumber)
            => $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{streetNamePuri}' en huisnummer '{houseNumber}'.";

        public static string PostalCodeDoesNotExist(string postInfoPuri)
            => $"De postinfo '{postInfoPuri}' is niet gekend in het postinforegister.";

        public const string HouseNumberInvalid = "Ongeldig huisnummerformaat.";

        public const string BoxNumberInvalid = "Ongeldig busnummerformaat.";

        public const string AddressNotFound = "Onbestaand adres.";
        public const string AddressRemoved = "Verwijderde adres.";
        public const string AddressCannotBeApproved = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
        public const string AddressCannotBeRejected = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
        public const string AddressCannotBeDeregulated = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
        public const string AddressCannotBeRegularized = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
        public const string AddressCannotBeRetired = "Deze actie is enkel toegestaan op adressen met status 'ingebruik'.";

        public const string PostalCodeNotInMunicipality = "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.";
    }
}
