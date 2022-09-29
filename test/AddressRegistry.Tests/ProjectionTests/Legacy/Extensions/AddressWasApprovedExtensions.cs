namespace AddressRegistry.Tests.ProjectionTests.Legacy.Extensions
{
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

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
