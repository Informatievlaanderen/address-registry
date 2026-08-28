namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressPositionCrsWasChangedExtensions
    {
        public static AddressPositionCrsWasChanged WithAddressPersistentLocalId(
            this AddressPositionCrsWasChanged @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressPositionCrsWasChanged(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressPositionCrsWasChanged WithExtendedWkbGeometry(
            this AddressPositionCrsWasChanged @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var newEvent = new AddressPositionCrsWasChanged(
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
