namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    public class OsloAssignmentDate : InstantValueObject<OsloAssignmentDate>
    {
        public OsloAssignmentDate([JsonProperty("value")] Instant assignmentDate) : base(assignmentDate) { }
    }
}
