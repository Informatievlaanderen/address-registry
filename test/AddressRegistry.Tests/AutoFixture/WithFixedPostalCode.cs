namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithFixedPostalCode : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var postalCodeAsString = "9000";
            fixture.Customize<PostalCode>(c => c.FromFactory(() => new PostalCode(postalCodeAsString)));
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(postalCodeAsString),
                    new ParameterSpecification(
                        typeof(string),
                        "postalCode")));
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(postalCodeAsString),
                    new ParameterSpecification(
                        typeof(string),
                        "sourcePostalCode")));
        }
    }
}
