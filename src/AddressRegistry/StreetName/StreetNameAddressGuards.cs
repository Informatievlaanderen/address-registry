namespace AddressRegistry.StreetName
{
    using Exceptions;
    using System.Linq;

    public partial class StreetNameAddress
    {
        private void GuardNotRemovedAddress()
        {
            if (IsRemoved)
            {
                throw new AddressIsRemovedException(AddressPersistentLocalId);
            }
        }

        public static void GuardGeometry(
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification)
        {
            if (geometryMethod == GeometryMethod.Interpolated)
            {
                throw new AddressHasInvalidGeometryMethodException();
            }

            if (geometryMethod == GeometryMethod.DerivedFromObject)
            {
                GuardsWhenDerivedFromObject(geometrySpecification);
            }

            if (geometryMethod == GeometryMethod.AppointedByAdministrator)
            {
                GuardsWhenAppointedByAdministrator(geometrySpecification);
            }
        }

        private static void GuardsWhenDerivedFromObject(GeometrySpecification geometrySpecification)
        {
            var validSpecifications = new[]
            {
                GeometrySpecification.Municipality
            };

            if (!validSpecifications.Contains(geometrySpecification))
            {
                throw new AddressHasInvalidGeometrySpecificationException();
            }
        }

        private static void GuardsWhenAppointedByAdministrator(GeometrySpecification geometrySpecification)
        {
            var validSpecifications = new[]
            {
                GeometrySpecification.Entry,
                GeometrySpecification.Parcel,
                GeometrySpecification.Lot,
                GeometrySpecification.Stand,
                GeometrySpecification.Berth,
                GeometrySpecification.BuildingUnit
            };

            if (!validSpecifications.Contains(geometrySpecification))
            {
                throw new AddressHasInvalidGeometrySpecificationException();
            }
        }
    }
}
