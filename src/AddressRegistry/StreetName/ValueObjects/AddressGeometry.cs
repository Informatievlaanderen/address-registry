namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class AddressGeometry : ValueObject<AddressGeometry>
    {
        public GeometryMethod GeometryMethod { get; }

        public GeometrySpecification GeometrySpecification { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public AddressGeometry(
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry geometry)
        {
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            Geometry = geometry;
        }

        public AddressGeometry(
            Address.GeometryMethod geometryMethod,
            Address.GeometrySpecification geometrySpecification,
            Address.ExtendedWkbGeometry geometry)
            : this((GeometryMethod)geometryMethod, (GeometrySpecification)geometrySpecification, new ExtendedWkbGeometry((byte[])geometry))
        { }

        protected override IEnumerable<object> Reflect()
        {
            yield return GeometryMethod;
            yield return GeometrySpecification;
            yield return Geometry;
        }
    }
}
