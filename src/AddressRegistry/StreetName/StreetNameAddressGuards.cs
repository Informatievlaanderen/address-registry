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
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position)
        {
            if (geometryMethod == GeometryMethod.Interpolated)
            {
                throw new AddressHasInvalidGeometryMethodException();
            }

            if (geometryMethod == GeometryMethod.AppointedByAdministrator)
            {
                GuardsWhenAppointedByAdministrator(geometrySpecification, position);
            }
        }

        private static void GuardsWhenAppointedByAdministrator(
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position)
        {
            if (position is null)
            {
                throw new AddressHasMissingPositionException();
            }

            if (geometrySpecification is null)
            {
                throw new AddressHasMissingGeometrySpecificationException();
            }

            var validSpecifications = new[]
            {
                GeometrySpecification.Entry,
                GeometrySpecification.Parcel,
                GeometrySpecification.Lot,
                GeometrySpecification.Stand,
                GeometrySpecification.Berth
            };

            if (!validSpecifications.Contains(geometrySpecification.Value))
            {
                throw new AddressHasInvalidGeometrySpecificationException();
            }
        }
    }
}
