namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressOfficialAssignmentWasRemoved")]
    [EventDescription("De (niet) officiele toekenning werd verwijderd.")]
    public class AddressOfficialAssignmentWasRemoved : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressOfficialAssignmentWasRemoved(AddressId addressId) => AddressId = addressId;

        [JsonConstructor]
        private AddressOfficialAssignmentWasRemoved(
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
