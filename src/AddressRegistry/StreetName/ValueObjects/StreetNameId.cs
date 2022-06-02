namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class StreetNameId : GuidValueObject<StreetNameId>
    {
        private const string Prefix = "streetName";

        public static StreetNameId CreateFor(string streetNameId)
            => new StreetNameId(Guid.Parse(streetNameId));

        public StreetNameId(Guid streetNameId) : base(streetNameId) { }

        public override string ToString() => $"{Prefix}/{Value}";
    }
}
