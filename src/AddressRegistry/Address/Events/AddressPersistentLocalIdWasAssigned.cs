namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using System;

    [EventName("AddressPersistentLocalIdentifierWasAssigned")]
    [EventDescription("Het adres kreeg een persistente lokale identifier toegekend.")]
    public class AddressPersistentLocalIdWasAssigned
    {
        public Guid AddressId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }

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
            Instant assignmentDate)
            : this(
                new AddressId(addressId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate)) { }
    }
}
