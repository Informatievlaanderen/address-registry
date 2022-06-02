namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BoxNumber : StringValueObject<BoxNumber>
    {
        public BoxNumber(string boxNumber) : base(boxNumber.RemoveUnicodeControlCharacters()) { }
    }
}
