namespace AddressRegistry.Tests.AutoFixture
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;

    public class WithFixedStreetNameId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var streetNameId = fixture.Create<CrabStreetNameId>();
            fixture.Register(() => streetNameId);
            fixture.Register(() => streetNameId.CreateDeterministicId());
        }
    }
}
