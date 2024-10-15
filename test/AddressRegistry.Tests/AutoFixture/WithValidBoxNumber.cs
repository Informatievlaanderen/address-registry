namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithValidBoxNumber : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<BoxNumber>(c =>
                c.FromFactory(() =>
                    new BoxNumber(fixture.Create<int>().ToString())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new SpecimenFactory<string>(() => fixture.Create<int>().ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "boxNumber")));
        }
    }
}
