namespace AddressRegistry.Tests.AutoFixture
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;

    public class WithFixedCrabHouseNumberId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var boxNumber = fixture.Create<CrabHouseNumberId>();
            fixture.Register(() => boxNumber);
        }
    }
}