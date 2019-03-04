namespace AddressRegistry.Address.Events
{
    using AddressRegistry;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressWasCorrectedToCurrent")]
    [EventDescription("Het adres werd in gebruik genomen via correctie.")]
    public class AddressWasCorrectedToCurrent : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
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
