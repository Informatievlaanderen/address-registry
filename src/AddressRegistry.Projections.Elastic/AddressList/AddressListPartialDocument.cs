namespace AddressRegistry.Projections.Elastic.AddressList
{
    using System;
    using System.Text.Json.Serialization;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;

    public class AddressListPartialDocument
    {
        /// <summary>
        /// Left unset by an update that does not change the object itself — the Lambert 2008
        /// reprojection — so the field is omitted from the partial update and Elastic keeps the
        /// version it already holds.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTimeOffset? VersionTimestamp { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AddressStatus? Status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? OfficiallyAssigned { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AddressPosition? AddressPosition { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? BoxNumber { get; set; }

        public AddressListPartialDocument(Instant versionTimestamp)
            : this(versionTimestamp.ToBelgianDateTimeOffset())
        {
        }

        public AddressListPartialDocument(DateTimeOffset versionTimestamp)
        {
            VersionTimestamp = versionTimestamp;
        }

        /// <summary>An update that must not touch the document's version.</summary>
        public AddressListPartialDocument()
        {
        }
    }
}
