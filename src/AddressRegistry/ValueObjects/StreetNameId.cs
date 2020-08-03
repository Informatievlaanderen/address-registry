namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;
    using System;

    public class StreetNameId : GuidValueObject<StreetNameId>
    {
        private const string Prefix = "streetName";

        public static StreetNameId CreateFor(CrabStreetNameId crabStreetNameId)
            => new StreetNameId(crabStreetNameId.CreateDeterministicId());

        // TODO: Refactor parsing of id, something with a url class?
        public static StreetNameId CreateForPersistentId(string persistentId)
            => CreateFor(
                new CrabStreetNameId(
                    int.Parse(persistentId.Replace("https://data.vlaanderen.be/id/straatnaam/", string.Empty))));

        public StreetNameId([JsonProperty("value")] Guid streetNameId) : base(streetNameId) { }

        public override string ToString() => $"{Prefix}/{Value}";
    }
}
