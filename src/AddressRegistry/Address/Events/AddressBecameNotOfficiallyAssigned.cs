namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressBecameNotOfficiallyAssigned")]
    [EventDescription("Het adres werd niet officieel toegekend.")]
    public class AddressBecameNotOfficiallyAssigned : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressBecameNotOfficiallyAssigned(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressBecameNotOfficiallyAssigned(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
