namespace AddressRegistry
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class PostalCode : StringValueObject<PostalCode>
    {
        // TODO: Use IdentifierUri instead of Uri as parameter
        public static PostalCode CreateForPersistentId(Uri persistentId)
            => new PostalCode(Path.GetFileName(persistentId.AbsolutePath));

        public PostalCode([JsonProperty("value")] string postCode) : base(postCode) { }
    }
}
