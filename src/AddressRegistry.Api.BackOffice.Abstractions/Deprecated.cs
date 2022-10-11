namespace AddressRegistry.Api.BackOffice.Abstractions
{
    public static class Deprecated
    {
        public static class Address
        {
            public const string AddressHouseNumberUnknown = "AdresActiefHuisNummerNietGekendValidatie";

            public const string PostalCodeDoesNotExist = "AdresPostinfoNietGekendValidatie";

            public const string HouseNumberOfBoxNumberCannotBeChanged = "AdresCorrectieHuisnummermetBusnummer";

            public const string HasNoBoxNumber = "AdresHuisnummerZonderBusnummer";

            public const string AddressCannotBeDeregulatedBecauseOfParent = "AdresHuisnummerVoorgesteldGehistoreerdOfAfgekeurd";

            public const string AddressCannotBeRegularized = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressCannotBeRetired = "AdresVoorgesteldOfAfgekeurd";
            public const string AddressPositionCannotBeChanged = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressPostalCodeCannotBeChanged = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressCannotCorrectRetirement = "AdresVoorgesteldOfAfgekeurd";
            public const string AddressIsNotOfficiallyAssigned = "AdresNietOfficeeltoegekend";
            public const string CannotBeCorrectToProposedFromRejected = "AdresInGebruikOfGehistoreerd";

            public const string AddressRejectionCannotBeCorrectedBecauseParentInvalidStatus =
                "AdresHuisnummerVoorgesteldAfgekeurdOfGehistoreerd";

            public const string AddressRetirementCannotBeCorrectedBecauseParentInvalidStatus =
                AddressRejectionCannotBeCorrectedBecauseParentInvalidStatus;

            public const string PostalCodeNotInMunicipality = "AdresPostinfoNietInGemeente";
        }
    }

    public static class ValidationErrorMessages
    {
        public static class Address
        {
            public static string AddressHouseNumberUnknown(string streetNamePuri, string houseNumber)
                => $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{streetNamePuri}' en huisnummer '{houseNumber}'.";

            public static string PostalCodeDoesNotExist(string postInfoPuri)
                => $"De postinfo '{postInfoPuri}' is niet gekend in het postinforegister.";

            public const string HouseNumberOfBoxNumberCannotBeChanged = "Te corrigeren huisnummer mag geen busnummer bevatten.";

            public const string HasNoBoxNumber = "Het adres heeft geen te corrigeren busnummer.";

            public const string AddressCannotBeDeregulatedBecauseOfParent = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.";
            public const string AddressCannotBeRegularized = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressCannotBeRetired = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";
            public const string AddressPositionCannotBeChanged = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressPostalCodeCannotBeChanged = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressIsNotOfficiallyAssigned = "Deze actie is enkel toegestaan voor officieel toegekende adressen.";
            public const string CannotBeCorrectToProposedFromRejected = "Deze actie is enkel toegestaan op een adres met status 'afgekeurd'.";
            public const string AddressCannotCorrectRetirement = "Deze actie is enkel toegestaan op adressen met status 'gehistoreerd'.";

            public const string AddressRejectionCannotBeCorrectedBecauseParentInvalidStatus =
                "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.";

            public const string AddressRetirementCannotBeCorrectedBecauseParentInvalidStatus =
                AddressRejectionCannotBeCorrectedBecauseParentInvalidStatus;

            public const string PostalCodeNotInMunicipality = "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.";
        }
    }
}
