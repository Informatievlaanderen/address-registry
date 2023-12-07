namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressPositionWasCorrectedV2Extensions
    {
        public static AddressPositionWasCorrectedV2 WithAddressPersistentLocalId(
            this AddressPositionWasCorrectedV2 @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressPositionWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressPositionWasCorrectedV2 WithExtendedWkbGeometry(
            this AddressPositionWasCorrectedV2 @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var newEvent = new AddressPositionWasCorrectedV2(
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
