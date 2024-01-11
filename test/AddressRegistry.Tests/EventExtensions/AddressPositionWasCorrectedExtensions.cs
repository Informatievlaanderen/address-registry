namespace AddressRegistry.Tests.EventExtensions
{
    using System;
    using Address;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class AddressPositionWasCorrectedExtensions
    {
        public static AddressPositionWasCorrected WithAddressId(
            this AddressPositionWasCorrected @event,
            Guid addressId)
        {
            var geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            var newEvent = new AddressPositionWasCorrected(
                new AddressId(addressId),
                geometry);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressPositionWasCorrected WithExtendedWkbGeometry(
            this AddressPositionWasCorrected @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                extendedWkbGeometry);
            var newEvent = new AddressPositionWasCorrected(
                new AddressId(@event.AddressId),
                geometry);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
