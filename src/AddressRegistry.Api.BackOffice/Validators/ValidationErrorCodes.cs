namespace AddressRegistry.Api.BackOffice.Validators
{
    public static class ValidationErrorCodes
    {
        public const string StreetNameInvalid = "AdresStraatnaamNietGekendValidatie";
        public const string StreetNameIsNotActive = "AdresStraatnaamGehistoreerdOfAfgekeurd";

        public const string AddressAlreadyExists = "AdresBestaandeHuisnummerBusnummerCombinatie";
        public const string AddressHouseNumberUnknown = "AdresActiefHuisNummerNietGekendValidatie";

        public const string PostalCodeDoesNotExist = "AdresPostinfoNietGekendValidatie";

        public const string HouseNumberInvalid = "AdresOngeldigHuisnummerformaat";

        public const string BoxNumberInvalid = "AdresOngeldigBusnummerformaat";

        public const string PositionSpecificationRequired = "AdresspecificatieVerplichtBijManueleAanduiding";

        public const string PositionSpecificationInvalid = "AdresspecificatieValidatie";

        public const string AddressRemoved = "AdresIsVerwijderd";
        public const string AddressCannotBeApproved = "AdresGehistoreerdOfAfgekeurd";
        public const string AddressCannotBeRejected = "AdresGehistoreerdOfInGebruik";
        public const string AddressCannotBeDeregulated = "AdresGehistoreerdOfAfgekeurd";
        public const string AddressCannotBeRegularized = "AdresGehistoreerdOfAfgekeurd";
        public const string AddressCannotBeRetired = "AdresVoorgesteldOfAfgekeurd";

        public const string PostalCodeNotInMunicipality = "AdresPostinfoNietInGemeente";
    }
}
