namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;
    using Newtonsoft.Json;
    using System;

    public class StreetNameId : GuidValueObject<StreetNameId>
    {
        private const string Prefix = "streetName";

        public static StreetNameId CreateFor(CrabStreetNameId crabStreetNameId)
            => new StreetNameId(crabStreetNameId.CreateDeterministicId());

        public static StreetNameId CreateForPersistentId(IdentifierUri<int> persistentId)
            => CreateFor(new CrabStreetNameId(persistentId.Value));

        public StreetNameId([JsonProperty("value")] Guid streetNameId) : base(streetNameId) { }

        public override string ToString() => $"{Prefix}/{Value}";
    }
}
