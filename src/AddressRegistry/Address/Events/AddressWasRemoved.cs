namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressWasRemoved")]
    [EventDescription("Het adres werd verwijderd.")]
    public class AddressWasRemoved : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasRemoved(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressWasRemoved(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
