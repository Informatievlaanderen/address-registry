namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class StreetNameWasImportedExtensions
    {
        public static StreetNameWasImported WithMunicipalityId(this StreetNameWasImported @event, MunicipalityId municipalityId)
        {
            var newEvent = new StreetNameWasImported(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                municipalityId,
                @event.StreetNameStatus);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasImported WithStreetNamePersistentLocalId(this StreetNameWasImported @event, StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var newEvent = new StreetNameWasImported(
                streetNamePersistentLocalId,
                new MunicipalityId(@event.MunicipalityId),
                @event.StreetNameStatus);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasImported WithStatus(this StreetNameWasImported @event, StreetNameStatus status)
        {
            var newEvent = new StreetNameWasImported(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new MunicipalityId(@event.MunicipalityId),
                status);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
