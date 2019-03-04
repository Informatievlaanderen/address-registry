namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using System;

    [EventName("AddressOsloIdWasAssigned")]
    [EventDescription("Het adres kreeg een Oslo Id toegekend.")]
    public class AddressOsloIdWasAssigned
    {
        public Guid AddressId { get; }
        public int OsloId { get; }
        public Instant AssignmentDate { get; }

        public AddressOsloIdWasAssigned(
            AddressId addressId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            AddressId = addressId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private AddressOsloIdWasAssigned(
            Guid addressId,
            int osloId,
            Instant assignmentDate)
            : this(
                new AddressId(addressId),
                new OsloId(osloId),
                new OsloAssignmentDate(assignmentDate)) { }
    }
}
