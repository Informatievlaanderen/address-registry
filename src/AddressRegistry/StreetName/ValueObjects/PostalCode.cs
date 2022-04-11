namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;
    using Newtonsoft.Json;

    public class PostalCode : StringValueObject<PostalCode>
    {
        public static PostalCode CreateForPersistentId(IdentifierUri<string> persistentId)
            => new PostalCode(persistentId.Value);

        public PostalCode([JsonProperty("value")] string postCode) : base(postCode) { }
    }
}
