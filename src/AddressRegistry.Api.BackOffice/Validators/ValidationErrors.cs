namespace AddressRegistry.Api.BackOffice.Validators
{
    public static class ValidationErrors
    {
        public static class StreetName
        {
            public const string StreetNameInvalid = "AdresStraatnaamNietGekendValidatie";
            public const string StreetNameIsNotActive = "AdresStraatnaamGehistoreerdOfAfgekeurd";
        }
        public static class Address
        {
            public const string AddressAlreadyExists = "AdresBestaandeHuisnummerBusnummerCombinatie";
            public const string AddressHouseNumberUnknown = "AdresActiefHuisNummerNietGekendValidatie";

            public const string PostalCodeDoesNotExist = "AdresPostinfoNietGekendValidatie";

            public const string HouseNumberInvalid = "AdresOngeldigHuisnummerformaat";

            public const string BoxNumberInvalid = "AdresOngeldigBusnummerformaat";

            public const string PositionSpecificationRequired = "AdresspecificatieVerplichtBijManueleAanduiding";
            public const string PositionSpecificationInvalid = "AdresspecificatieValidatie";

            public const string PositionRequired = "AdresGeometriemethodeValidatie";
            public const string PositionInvalidFormat = "AdrespositieFormaatValidatie";

            public const string AddressRemoved = "AdresIsVerwijderd";
            public const string AddressCannotBeApproved = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressCannotBeRejected = "AdresGehistoreerdOfInGebruik";
            public const string AddressCannotBeDeregulated = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressCannotBeRegularized = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressCannotBeRetired = "AdresVoorgesteldOfAfgekeurd";

            public const string PostalCodeNotInMunicipality = "AdresPostinfoNietInGemeente";
        }
    }

    public static class ValidationErrorMessages
    {
        public static class StreetName
        {
            public static string StreetNameInvalid(string streetNamePuri)
                => $"De straatnaam '{streetNamePuri}' is niet gekend in het straatnaamregister.";

            public const string StreetNameIsNotActive = "De straatnaam is gehistoreerd of afgekeurd.";
        }
        public static class Address
        {
            public const string AddressAlreadyExists = "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.";

            public static string AddressHouseNumberUnknown(string streetNamePuri, string houseNumber)
                => $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{streetNamePuri}' en huisnummer '{houseNumber}'.";

            public static string PostalCodeDoesNotExist(string postInfoPuri)
                => $"De postinfo '{postInfoPuri}' is niet gekend in het postinforegister.";

            public const string HouseNumberInvalid = "Ongeldig huisnummerformaat.";

            public const string BoxNumberInvalid = "Ongeldig busnummerformaat.";

            public const string PositionSpecificationRequired = "Positiespecificatie is verplicht bij een manuele aanduiding van de positie.";
            public const string PositionSpecificationInvalid = "Ongeldige positiespecificatie.";

            public const string PositionRequired = "De parameter 'positie' is verplicht voor indien aangeduid door beheerder.";
            public const string PositionInvalidFormat = "De positie is geen geldige gml-puntgeometrie.";

            public const string AddressNotFound = "Onbestaand adres.";
            public const string AddressRemoved = "Verwijderde adres.";
            public const string AddressCannotBeApproved = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
            public const string AddressCannotBeRejected = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
            public const string AddressCannotBeDeregulated = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressCannotBeRegularized = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressCannotBeRetired = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";

            public const string PostalCodeNotInMunicipality = "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.";
        }
    }
}
