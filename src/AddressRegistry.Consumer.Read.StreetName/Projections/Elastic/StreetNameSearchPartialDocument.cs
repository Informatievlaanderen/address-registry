namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System;
    using System.Text.Json.Serialization;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;

    public class StreetNameSearchPartialDocument
    {
        public DateTimeOffset VersionTimestamp { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public StreetNameStatus? Status { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? Active => Status is not null
            ? Status is StreetNameStatus.Proposed or StreetNameStatus.Current
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
