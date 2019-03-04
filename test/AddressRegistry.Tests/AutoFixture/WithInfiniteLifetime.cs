namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;

    public class WithInfiniteLifetime : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<CrabLifetime>(c => c.FromFactory(
                () => new CrabLifetime(fixture.Create<LocalDateTime>(), null)));
        }
    }
}
