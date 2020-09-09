namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("AddressPersistentLocalIdentifierWasAssigned")]
    [EventDescription("Het adres kreeg een persistente lokale identifier toegekend.")]
    public class AddressPersistentLocalIdWasAssigned : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressPersistentLocalIdWasAssigned(
            AddressId addressId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            AddressId = addressId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private AddressPersistentLocalIdWasAssigned(
            Guid addressId,
            int persistentLocalId,
            Instant assignmentDate,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate))
            => ((ISetProvenance) this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
