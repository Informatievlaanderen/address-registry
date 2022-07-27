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

        public const string AddressRemoved = "AdresIsVerwijderd";
        public const string AddressCannotBeApproved = "AdresGehistoreerdOfAfgekeurd";
        public const string AddressCannotBeRejected = "AdresGehistoreerdOfGoedgekeurd";

        public const string PostalCodeNotInMunicipality = "AdresPostinfoNietInGemeente";
    }
}
