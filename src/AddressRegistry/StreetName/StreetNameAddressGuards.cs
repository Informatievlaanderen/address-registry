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

        private void GuardsWhenAppointedByAdministrator(
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
