namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressWasApprovedExtensions
    {
        public static AddressWasApproved WithStreetNamePersistentLocalId(
            this AddressWasApproved @event,
            StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var newEvent = new AddressWasApproved(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(@event.AddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasApproved WithAddressPersistentLocalId(
            this AddressWasApproved @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasApproved(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
