namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithValidHouseNumber : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<HouseNumber>(c =>
                c.FromFactory(() =>
                    new HouseNumber(fixture.Create<int>().ToString())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new SpecimenFactory<string>(() => fixture.Create<int>().ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "houseNumber")));
        }
    }
}
