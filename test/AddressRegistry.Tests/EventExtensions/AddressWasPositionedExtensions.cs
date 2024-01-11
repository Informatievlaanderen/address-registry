namespace AddressRegistry.Tests.EventExtensions
{
    using System;
    using Address;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class AddressWasPositionedExtensions
    {
        public static AddressWasPositioned WithAddressId(
            this AddressWasPositioned @event,
            Guid addressId)
        {
            var geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            var newEvent = new AddressWasPositioned(
                new AddressId(addressId),
                geometry);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasPositioned WithExtendedWkbGeometry(
            this AddressWasPositioned @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                extendedWkbGeometry);
            var newEvent = new AddressWasPositioned(
                new AddressId(@event.AddressId),
                geometry);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
