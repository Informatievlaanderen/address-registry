namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithValidHouseNumber : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var houseNumber = fixture.Create<int>().ToString();

            fixture.Customize<HouseNumber>(c => c.FromFactory(() => new HouseNumber(houseNumber)));
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(houseNumber),
                    new ParameterSpecification(
                        typeof(string),
                        "houseNumber")));
        }
    }
}
