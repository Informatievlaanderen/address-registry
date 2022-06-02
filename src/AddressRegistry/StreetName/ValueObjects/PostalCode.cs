namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;

    public class PostalCode : StringValueObject<PostalCode>
    {
        public static PostalCode CreateForPersistentId(IdentifierUri<string> persistentId)
            => new PostalCode(persistentId.Value);

        public PostalCode(string postCode) : base(postCode) { }
    }
}
