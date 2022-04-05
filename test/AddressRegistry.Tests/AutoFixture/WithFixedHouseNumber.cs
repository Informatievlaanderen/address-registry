namespace AddressRegistry.Tests.AutoFixture
{
    using Address.ValueObjects;
    using global::AutoFixture;

    public class WithFixedHouseNumber : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var houseNumber = fixture.Create<HouseNumber>();
            fixture.Register(() => houseNumber);
        }
    }
}
