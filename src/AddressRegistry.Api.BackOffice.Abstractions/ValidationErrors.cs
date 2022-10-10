namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using Amazon.DynamoDBv2;
    using Amazon.Runtime;

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
            public const string HouseNumberOfBoxNumberCannotBeChanged = "AdresCorrectieHuisnummermetBusnummer";

            public const string BoxNumberInvalid = "AdresOngeldigBusnummerformaat";
            public const string HasNoBoxNumber = "AdresHuisnummerZonderBusnummer";

            public const string PositionSpecificationRequired = "AdresPositieSpecificatieVerplichtBijManueleAanduiding";
            public const string PositionSpecificationInvalid = "AdresPositieSpecificatieValidatie";

            public const string PositionRequired = "AdresPositieGeometriemethodeValidatie";
            public const string PositionInvalidFormat = "AdresPositieformaatValidatie";
            public const string GeometryMethodInvalid = "AdresPositieGeometriemethodeValidatie";

            public const string AddressRemoved = "AdresIsVerwijderd";
            public const string AddressNotFound = "AdresIsOnbestaand";
            public const string AddressCannotBeApproved = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressApprovalCannotBeCorrected = "AdresGehistoreerdOfAfgekeurd";
            public const string AddressCannotBeApprovedBecauseOfParent = "AdresHuisnummerVoorgesteldGehistoreerdOfAfgekeurd";
            public const string AddressCannotBeRejected = "AdresGehistoreerdOfInGebruik";
            public const string AddressCannotBeDeregulated = "AdresGehistoreerdOfAfgekeurd";
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
            public const string HouseNumberOfBoxNumberCannotBeChanged = "Te corrigeren huisnummer mag geen busnummer bevatten.";

            public const string BoxNumberInvalid = "Ongeldig busnummerformaat.";
            public const string HasNoBoxNumber = "Het adres heeft geen te corrigeren busnummer.";

            public const string PositionSpecificationRequired = "PositieSpecificatie is verplicht bij een manuele aanduiding van de positie.";

            public const string PositionSpecificationInvalid = "Ongeldige positieSpecificatie.";

            public const string PositionRequired = "De parameter 'positie' is verplicht indien positieGeometrieMethode aangeduidDoorBeheerder is.";
            public const string PositionInvalidFormat = "De positie is geen geldige gml-puntgeometrie.";
            public const string GeometryMethodInvalid = "Ongeldige positieGeometrieMethode.";

            public const string AddressNotFound = "Onbestaand adres.";
            public const string AddressRemoved = "Verwijderde adres.";
            public const string AddressCannotBeApproved = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
            public const string AddressApprovalCannotBeCorrected = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";
            public const string AddressCannotBeApprovedBecauseOfParent = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.";
            public const string AddressCannotBeRejected = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
            public const string AddressCannotBeDeregulated = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressCannotBeDeregulatedBecauseOfParent = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.";
            public const string AddressCannotBeRegularized = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressCannotBeRetired = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";
            public const string AddressPositionCannotBeChanged = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressPostalCodeCannotBeChanged = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
            public const string AddressIsNotOfficiallyAssigned = "Deze actie is enkel toegestaan voor officieel toegekende adressen.";
            public const string CannotBeCorrectToProposedFromRejected = "Deze actie is enkel toegestaan op een adres met status 'afgekeurd'.";
            public const string AddressCannotCorrectRetirement = "Deze actie is enkel toegestaan op adressen met status 'gehistoreerd'.";

            public const string AddressRejectionCannotBeCorrectedBecauseParentInvalidStatus =
                "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status ‘inGebruik’ heeft.";

            public const string AddressRetirementCannotBeCorrectedBecauseParentInvalidStatus =
                AddressRejectionCannotBeCorrectedBecauseParentInvalidStatus;

            public const string PostalCodeNotInMunicipality = "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.";
        }
    }

    // Voorstel om errors te refactoren per feature
    public static class ValidationErrors2
    {
        public static class Common
        {
            public static class AddressAlreadyExists
            {
                public const string Code = "AdresBestaandeHuisnummerBusnummerCombinatie";
                public const string Message = "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.";
            }

            public static class AddressRemoved
            {
                public const string Message = "Verwijderde adres.";
            }

            public static class AddressNotFound
            {
                public const string Message = "Onbestaand adres.";
            }
        }

        public static class CorrectRejection
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerVoorgesteldAfgekeurdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status ‘inGebruik’ heeft.";
            }

            public static class AddressIncorrectStatus
            {
                public const string Code = "AdresInGebruikOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op een adres met status 'afgekeurd'.";
            }
        }

        public static class CorrectRetirement
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerVoorgesteldAfgekeurdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status ‘inGebruik’ heeft.";
            }

            public static class AddressIncorrectStatus
            {
                public const string Code = "AdresInGebruikOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op een adres met status 'afgekeurd'.";
            }
        }
    }
}
