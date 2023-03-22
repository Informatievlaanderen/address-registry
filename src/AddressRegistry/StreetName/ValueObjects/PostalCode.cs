namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;

    public class PostalCode : StringValueObject<PostalCode>
    {
        public static PostalCode CreateForPersistentId(IdentifierUri<string> persistentId)
            => new PostalCode(persistentId.Value);

        public PostalCode(string postalCode) : base(postalCode)
        {
            if (string.IsNullOrEmpty(postalCode))
            {
                throw new ArgumentNullException(nameof(postalCode));
            }
        }
    }
}
