namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using System.Linq;
    using Address.ValueObjects;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;

    public class WithStreetNameIdExcept : ICustomization
    {
        private readonly Guid _streetNameId;

        public WithStreetNameIdExcept(Guid streetNameId) => _streetNameId = streetNameId;

        public void Customize(IFixture fixture)
        {
            var crabStreetNameId = fixture.Create<Generator<CrabStreetNameId>>()
                .FirstOrDefault(id => id.CreateDeterministicId() != _streetNameId);
            var streetNameId = fixture.Create<Generator<StreetNameId>>()
                .FirstOrDefault(id => id != _streetNameId);

            fixture.Register(() => crabStreetNameId);
            fixture.Register(() => streetNameId);
        }
    }
}
