namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressStatusWasCorrectedToRemoved")]
    [EventDescription("De adresstatus werd verwijderd (via correctie).")]
    public class AddressStatusWasCorrectedToRemoved : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressStatusWasCorrectedToRemoved(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressStatusWasCorrectedToRemoved(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
