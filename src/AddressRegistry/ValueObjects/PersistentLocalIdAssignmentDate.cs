namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    public class PersistentLocalIdAssignmentDate : InstantValueObject<PersistentLocalIdAssignmentDate>
    {
        public PersistentLocalIdAssignmentDate([JsonProperty("value")] Instant assignmentDate) : base(assignmentDate) { }
    }
}
