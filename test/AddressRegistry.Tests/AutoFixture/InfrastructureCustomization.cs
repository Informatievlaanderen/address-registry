namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;

    public class InfrastructureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize(new NodaTimeCustomization());
            fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
        }
    }
}
