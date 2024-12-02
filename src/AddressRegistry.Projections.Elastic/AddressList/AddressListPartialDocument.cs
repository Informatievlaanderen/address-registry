﻿namespace AddressRegistry.Projections.Elastic.AddressList
{
    using System;
    using System.Text.Json.Serialization;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;

    public class AddressListPartialDocument
    {
        public DateTimeOffset VersionTimestamp { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AddressStatus? Status { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? Active => Status is not null
            ? Status is AddressStatus.Proposed or AddressStatus.Current
            : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? OfficiallyAssigned { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AddressPosition? AddressPosition { get; set; }

        public AddressListPartialDocument(Instant versionTimestamp)
            : this(versionTimestamp.ToBelgianDateTimeOffset())
        {
        }

        public AddressListPartialDocument(DateTimeOffset versionTimestamp)
        {
            VersionTimestamp = versionTimestamp;
        }
    }
}
