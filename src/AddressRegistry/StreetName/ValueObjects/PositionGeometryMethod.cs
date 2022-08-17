namespace AddressRegistry.StreetName
{
    using System;

    public class PositionGeometryMethod
    {
        public static readonly PositionGeometryMethod AppointedByAdministrator = new PositionGeometryMethod("AppointedByAdministrator");
        public static readonly PositionGeometryMethod DerivedFromObject = new PositionGeometryMethod("DerivedFromObject");

        public string GeometryMethod { get; }

        private PositionGeometryMethod(string geometryMethod) => GeometryMethod = geometryMethod;

        public static PositionGeometryMethod Parse(string geometryMethod)
        {
            if (geometryMethod != AppointedByAdministrator.GeometryMethod &&
                geometryMethod != DerivedFromObject.GeometryMethod)
                throw new NotImplementedException($"Cannot parse {geometryMethod} to PositionGeometryMethod");

            return new PositionGeometryMethod(geometryMethod);
        }

        public static implicit operator string(PositionGeometryMethod positionGeometryMethod) => positionGeometryMethod.GeometryMethod;
    }
}
