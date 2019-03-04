namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressBecameComplete")]
    [EventDescription("Het adres werd vervolledigd.")]
    public class AddressBecameComplete : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressBecameComplete(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressBecameComplete(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
