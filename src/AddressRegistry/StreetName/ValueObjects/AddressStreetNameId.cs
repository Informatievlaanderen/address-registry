namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo;

    public class AddressStreetNameId : GuidValueObject<AddressStreetNameId>
    {
        private const string Prefix = "streetName";

        public static AddressStreetNameId CreateFor(CrabStreetNameId crabStreetNameId)
            => new AddressStreetNameId(crabStreetNameId.CreateDeterministicId());

        public static AddressStreetNameId CreateForPersistentId(IdentifierUri<int> persistentId)
            => CreateFor(new CrabStreetNameId(persistentId.Value));

        public AddressStreetNameId(Guid streetNameId) : base(streetNameId) { }

        public override string ToString() => $"{Prefix}/{Value}";
    }

}
