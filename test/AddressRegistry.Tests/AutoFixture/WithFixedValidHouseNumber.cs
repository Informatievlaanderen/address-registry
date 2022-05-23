namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithFixedValidHouseNumber : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var houseNumberStr = "11A";
            fixture.Customize<HouseNumber>(c => c.FromFactory(() => new HouseNumber(houseNumberStr)));
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(houseNumberStr),
                    new ParameterSpecification(
                        typeof(string),
                        "houseNumber")));
        }
    }
}
