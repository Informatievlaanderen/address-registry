namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class HouseNumber : StringValueObject<HouseNumber>
    {
        public HouseNumber([JsonProperty("value")] string houseNumber) : base(houseNumber.RemoveUnicodeControlCharacters()) { }
    }
}
