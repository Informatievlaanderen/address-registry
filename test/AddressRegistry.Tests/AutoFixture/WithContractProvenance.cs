namespace AddressRegistry.Tests.AutoFixture
{
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using global::AutoFixture;
    using NodaTime;

    public class WithContractProvenance : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize(new NodaTimeCustomization());
            fixture.Customize<Provenance>(c => c.FromFactory(
                () => new Provenance(
                    fixture.Create<Instant>().ToString(),
                    fixture.Create<string>(),
                    fixture.Create<string>(),
                    fixture.Create<string>(),
                    fixture.Create<string>())));
        }
    }
}
