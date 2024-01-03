namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using StreetName;

    public class WithIntegerNisCode : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var nisCode = new NisCode(fixture.Create<int>().ToString());
            fixture.Register(() => nisCode);

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(nisCode.ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "nisCode")));
        }
    }
}
