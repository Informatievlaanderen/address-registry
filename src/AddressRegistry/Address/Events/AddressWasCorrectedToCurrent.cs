namespace AddressRegistry.Address.Events
{
    using AddressRegistry;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressWasCorrectedToCurrent")]
    [EventDescription("Het adres kreeg status 'in gebruik' (via correctie).")]
    public class AddressWasCorrectedToCurrent : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasCorrectedToCurrent(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressWasCorrectedToCurrent(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
