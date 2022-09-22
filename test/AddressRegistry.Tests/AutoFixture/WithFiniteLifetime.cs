namespace AddressRegistry.Tests.AutoFixture
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;

    public class WithFiniteLifetime : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<CrabLifetime>(c => c.FromFactory(
                () => new CrabLifetime(fixture.Create<LocalDateTime>(), fixture.Create<LocalDateTime>())));
        }
    }
}
