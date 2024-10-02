namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System;
    using System.Text.Json.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;

    public class StreetNameSearchPartialDocument
    {
        public DateTimeOffset VersionTimestamp { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AddressRegistry.StreetName.StreetNameStatus? Status { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? Active => Status is not null
            ? Status is AddressRegistry.StreetName.StreetNameStatus.Proposed or AddressRegistry.StreetName.StreetNameStatus.Current
            : null;

        public StreetNameSearchPartialDocument(Instant versionTimestamp)
            : this(versionTimestamp.ToBelgianDateTimeOffset())
        { }

        public StreetNameSearchPartialDocument(DateTimeOffset versionTimestamp)
        {
            VersionTimestamp = versionTimestamp;
        }
    }
}
