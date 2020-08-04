namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;
    using System;
    using System.IO;

    public class StreetNameId : GuidValueObject<StreetNameId>
    {
        private const string Prefix = "streetName";

        public static StreetNameId CreateFor(CrabStreetNameId crabStreetNameId)
            => new StreetNameId(crabStreetNameId.CreateDeterministicId());

        // TODO: Use IdentifierUri instead of Uri as parameter
        public static StreetNameId CreateForPersistentId(Uri persistentId)
            => CreateFor(new CrabStreetNameId(int.Parse(Path.GetFileName(persistentId.AbsolutePath))));

        public StreetNameId([JsonProperty("value")] Guid streetNameId) : base(streetNameId) { }

        public override string ToString() => $"{Prefix}/{Value}";
    }
}
