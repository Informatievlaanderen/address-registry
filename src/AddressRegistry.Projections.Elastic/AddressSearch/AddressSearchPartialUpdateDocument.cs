namespace AddressRegistry.Projections.Elastic.AddressSearch
{
    using System;
    using System.Text.Json.Serialization;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;

    //TODO-rik add test to ensure properties are the same as in AddressSearchDocument
    //TODO-rik add test to ensure only filled in properties are serialized
    public class AddressSearchPartialUpdateDocument
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
        public string? PostalCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? HouseNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? BoxNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AddressPosition? AddressPosition { get; set; }

        public AddressSearchPartialUpdateDocument(Instant versionTimestamp)
        {
            VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();
        }
    }
}
