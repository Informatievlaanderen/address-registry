namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class BoxNumber : StringValueObject<BoxNumber>
    {
        public BoxNumber([JsonProperty("value")] string boxNumber) : base(boxNumber.RemoveUnicodeControlCharacters()) { }
    }
}
