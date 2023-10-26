namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressWasRejectedExtensions
    {
        public static AddressWasRejected WithAddressPersistentLocalId(
            this AddressWasRejected @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasRejected(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
