namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;

    public class WithFixedSubaddressId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var houseNumberId = fixture.Create<int>();
            fixture.Customize<CrabSubaddressId>(c => c.FromFactory(() => new CrabSubaddressId(houseNumberId)));
            fixture.Customize<AddressId>(c => c.FromFactory(() => AddressId.CreateFor(fixture.Create<CrabSubaddressId>())));
        }
    }
}