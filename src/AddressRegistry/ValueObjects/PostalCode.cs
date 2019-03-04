namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class PostalCode : StringValueObject<PostalCode>
    {
        public PostalCode([JsonProperty("value")] string postCode) : base(postCode) { }
    }
}
