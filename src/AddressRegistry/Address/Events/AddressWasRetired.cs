namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressWasRetired")]
    [EventDescription("Het adres werd gehistoreerd.")]
    public class AddressWasRetired : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressWasRetired(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressWasRetired(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
