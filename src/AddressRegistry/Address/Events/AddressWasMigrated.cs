namespace AddressRegistry.Address.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("AddressWasMigrated")]
    [EventDescription("Het adres werd gemigreerd.")]
    public class AddressWasMigrated : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam naar waar het adres is verplaatst.")]
        public int StreetNamePersistentLocalId { get; set; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasMigrated(
            AddressId addressId,
            StreetName.StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            AddressId = addressId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
        }

        [JsonConstructor]
        private AddressWasMigrated(
            Guid addressId,
            int streetNamePersistentLocalId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new StreetName.StreetNamePersistentLocalId(streetNamePersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
