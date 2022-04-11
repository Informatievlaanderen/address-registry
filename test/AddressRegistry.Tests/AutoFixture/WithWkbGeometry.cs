namespace AddressRegistry.Tests.AutoFixture
{
    using Address;
    using global::AutoFixture;

    public class WithWkbGeometry : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(
                () => GeometryHelpers.CreateFromWkt($"POINT ({fixture.Create<uint>()} {fixture.Create<uint>()})")));
        }
    }
}
