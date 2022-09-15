namespace AddressRegistry.Api.BackOffice.Validators
{
    using StreetName;

    public static class HouseNumberValidator
    {
        public static bool IsValid(string houseNumber)
            => HouseNumber.HasValidFormat(houseNumber);
    }
}
