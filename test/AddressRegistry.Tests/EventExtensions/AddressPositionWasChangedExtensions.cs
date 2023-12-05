namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressPositionWasChangedExtensions
    {
        public static AddressPositionWasChanged WithAddressPersistentLocalId(
            this AddressPositionWasChanged @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressPositionWasChanged(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressPositionWasChanged WithExtendedWkbGeometry(
            this AddressPositionWasChanged @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var newEvent = new AddressPositionWasChanged(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.GeometryMethod,
                @event.GeometrySpecification,
                extendedWkbGeometry);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
