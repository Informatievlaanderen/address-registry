namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class MigratedStreetNameWasImportedExtensions
    {
        public static MigratedStreetNameWasImported WithStatus(this MigratedStreetNameWasImported @event, StreetNameStatus status)
        {
            var newEvent = new MigratedStreetNameWasImported(
                new StreetNameId(@event.StreetNameId),
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new MunicipalityId(@event.MunicipalityId),
                new NisCode(@event.NisCode),
                status);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
