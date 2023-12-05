namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("AddressPersistentLocalIdentifierWasAssigned")]
    [EventDescription("Het adres kreeg een persistente lokale identificator toegekend.")]
    public class AddressPersistentLocalIdWasAssigned : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Tijdstip waarop de objectidentificator van het adres werd toegekend.")]
        public Instant AssignmentDate { get; }

        [EventPropertyDescription("Metadata bij het event.")]
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
