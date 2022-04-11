namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithExtendedWkbGeometry : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.CreateEwkbFrom(
                GeometryHelpers.CreateFromWkt($"POINT ({fixture.Create<uint>()} {fixture.Create<uint>()})"));

            fixture.Customize<Address.ExtendedWkbGeometry>(c => c.FromFactory(
                () => extendedWkbGeometry));

            fixture.Customize<StreetName.ExtendedWkbGeometry>(c => c.FromFactory(
                () => new StreetName.ExtendedWkbGeometry(extendedWkbGeometry.ToString())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(extendedWkbGeometry.ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "extendedWkbGeometry")));
        }
    }
}
