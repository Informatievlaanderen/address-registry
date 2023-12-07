namespace AddressRegistry.Address
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
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

        protected override IEnumerable<object> Reflect()
        {
            yield return GeometryMethod;
            yield return GeometrySpecification;
            yield return Geometry;
        }
    }
}
