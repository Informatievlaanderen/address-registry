namespace AddressRegistry.Tests.AutoFixture
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using StreetName;

    /// <summary>
    /// Positions as the event store holds them once it is converted to Lambert 2008 (EPSG 3812),
    /// so projections can be tested against the reference system they will actually receive.
    /// The point is <see cref="GeometryHelpers.GmlPointGeometry"/> expressed in Lambert 2008.
    /// </summary>
    public class WithExtendedWkbGeometryLambert2008 : ICustomization
    {
        public const string PointWkt = "POINT (603668.87 692041.51)";

        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.CreateEwkbFromWkt(PointWkt, SystemReferenceId.SridLambert2008);

            fixture.Customize<Address.ExtendedWkbGeometry>(c => c.FromFactory(
                () => new Address.ExtendedWkbGeometry(extendedWkbGeometry.ToString())));

            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(
                () => new ExtendedWkbGeometry(extendedWkbGeometry.ToString())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(extendedWkbGeometry.ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "extendedWkbGeometry")));
        }
    }
}
