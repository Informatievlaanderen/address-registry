namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class HouseNumber : StringValueObject<HouseNumber>
    {
        public HouseNumber(string houseNumber) : base(houseNumber.RemoveUnicodeControlCharacters()) { }
    }
}
