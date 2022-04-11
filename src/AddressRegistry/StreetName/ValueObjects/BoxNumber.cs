namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class BoxNumber : StringValueObject<BoxNumber>
    {
        public BoxNumber([JsonProperty("value")] string boxNumber) : base(boxNumber.RemoveUnicodeControlCharacters()) { }
    }
}
