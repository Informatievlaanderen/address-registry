namespace AddressRegistry.Projections.Elastic.AddressSearch
{
    using System;
    using AddressRegistry.StreetName;
    using NodaTime;

    public class AddressSearchDocument
    {
        protected AddressSearchDocument()
        { }

        public AddressSearchDocument(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            int? parentAddressPersistentLocalId,
            string? postalCode,
            string houseNumber,
            string? boxNumber,
            AddressStatus status,
            bool officiallyAssigned,
            byte[] position,
            GeometryMethod positionMethod,
            GeometrySpecification positionSpecification,
            bool removed,
            Instant versionTimestamp)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            ParentAddressPersistentLocalId = parentAddressPersistentLocalId;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            Status = status;
            OfficiallyAssigned = officiallyAssigned;
            Position = position;
            PositionMethod = positionMethod;
            PositionSpecification = positionSpecification;
            Removed = removed;
            VersionTimestamp = versionTimestamp;
        }

        public int AddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public int? ParentAddressPersistentLocalId { get; set; }
        public string? PostalCode { get; set; }
        public string HouseNumber { get; set; } = string.Empty;
        public string? BoxNumber { get; set; }
        public AddressStatus Status { get; set; }
        public bool OfficiallyAssigned { get; set; }

        public byte[] Position { get; set; }
        public GeometryMethod PositionMethod { get; set; }
        public GeometrySpecification PositionSpecification { get; set; }

        public bool Removed { get; set; }

        public string LastEventHash { get; set; } = string.Empty;
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }
}
