namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class PostalCode : StringValueObject<PostalCode>
    {
        // TODO: Refactor parsing of id, something with a url class?
        public static PostalCode CreateForPersistentId(string persistentId)
            => new PostalCode(persistentId.Replace("https://data.vlaanderen.be/id/postinfo/", string.Empty));

        public PostalCode([JsonProperty("value")] string postCode) : base(postCode) { }
    }
}
