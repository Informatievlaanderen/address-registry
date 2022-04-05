namespace AddressRegistry.Tests.AutoFixture
{
    using Address.ValueObjects;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;

    public class WithFixedAddressId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var houseNumberId = fixture.Create<int>();
            fixture.Customize<CrabHouseNumberId>(c => c.FromFactory(() => new CrabHouseNumberId(houseNumberId)));
            fixture.Customize<AddressId>(c => c.FromFactory(() => AddressId.CreateFor(fixture.Create<CrabHouseNumberId>())));
        }
    }
}
