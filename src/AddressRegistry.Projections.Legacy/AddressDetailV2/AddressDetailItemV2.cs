namespace AddressRegistry.Projections.Legacy.AddressDetailV2
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using StreetName;

    public class AddressDetailItemV2
    {
        protected AddressDetailItemV2()
        { }

        public AddressDetailItemV2(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            string postalCode,
            string houseNumber,
            AddressStatus status,
            bool removed,
            Instant versionTimestamp)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            Status = status;
            Removed = removed;
            VersionTimestamp = versionTimestamp;
        }

        public AddressDetailItemV2(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            string postalCode,
            string houseNumber,
            string? boxNumber,
            AddressStatus status,
            bool officiallyAssigned,
            byte[]? position,
            GeometryMethod? positionMethod,
            GeometrySpecification? positionSpecification,
            bool removed,
            Instant versionTimestamp)
            : this(addressPersistentLocalId, streetNamePersistentLocalId, postalCode, houseNumber, status, removed, versionTimestamp)
        {
            BoxNumber = boxNumber;
            OfficiallyAssigned = officiallyAssigned;
            Position = position;
            PositionMethod = positionMethod;
            PositionSpecification = positionSpecification;
        }

        public int AddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string? BoxNumber { get; set; }
        public AddressStatus Status { get; set; }
        public bool OfficiallyAssigned { get; set; }

        public byte[]? Position { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public bool Removed { get; set; }

        public string LastEventHash { get; set; } = string.Empty;
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class AddressDetailItemV2Configuration : IEntityTypeConfiguration<AddressDetailItemV2>
    {
        internal const string TableName = "AddressDetailsV2";

        public void Configure(EntityTypeBuilder<AddressDetailItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressPersistentLocalId)
                .IsClustered();

            b.Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNamePersistentLocalId);
            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.OfficiallyAssigned);
            b.Property(p => p.Position);
            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);
            b.Property(x => x.LastEventHash);

            b.HasIndex(p => p.StreetNamePersistentLocalId);
            b.HasIndex(p => p.HouseNumber);
            b.HasIndex(p => p.BoxNumber);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.PostalCode);
            b.HasIndex(p => p.VersionTimestampAsDateTimeOffset);
            b.HasIndex(p => p.Removed);
        }
    }
}
