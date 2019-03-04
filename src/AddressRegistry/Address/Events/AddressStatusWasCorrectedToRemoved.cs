namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressStatusWasCorrectedToRemoved")]
    [EventDescription("Het adres werd verwijderd via correctie.")]
    public class AddressStatusWasCorrectedToRemoved : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
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
