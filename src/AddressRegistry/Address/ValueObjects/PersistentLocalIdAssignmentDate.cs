namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class PersistentLocalIdAssignmentDate : InstantValueObject<PersistentLocalIdAssignmentDate>
    {
        public PersistentLocalIdAssignmentDate([JsonProperty("value")] Instant assignmentDate) : base(assignmentDate) { }
    }
}
