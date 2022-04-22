namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using StreetName;

    public class WithFlemishNisCode : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var nisCode = "11001";
            fixture.Customize<NisCode>(c => c.FromFactory(
                () => new NisCode(nisCode)));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(nisCode),
                    new ParameterSpecification(
                        typeof(string),
                        "nisCode")));
        }
    }
}
