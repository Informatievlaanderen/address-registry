namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class PostalCode : StringValueObject<PostalCode>
    {
        public static PostalCode CreateForPersistentId(IdentifierUri<string> persistentId)
            => new PostalCode(persistentId.Value);

        public PostalCode([JsonProperty("value")] string postCode) : base(postCode) { }
    }
}
