namespace AddressRegistry.Projections.Wfs.AddressWfs
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class AddressWfsItem
    {
        protected AddressWfsItem()
        { }

        public AddressWfsItem(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            string postalCode,
            string houseNumber,
            string status,
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

        public AddressWfsItem(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            string postalCode,
            string houseNumber,
            string? boxNumber,
            string status,
            bool officiallyAssigned,
            Point? position,
            string? positionMethod,
            string? positionSpecification,
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
        public string PostalCode { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public string Status { get; set; }
        public bool OfficiallyAssigned { get; set; }

        public Point? Position { get; set; }
        public string? PositionMethod { get; set; }
        public string? PositionSpecification { get; set; }

        public bool Removed { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public string? VersionAsString { get; protected set; }
    }

    public class AddressWfsItemConfiguration : IEntityTypeConfiguration<AddressWfsItem>
    {
        internal const string TableName = "AddressWfs";

        public void Configure(EntityTypeBuilder<AddressWfsItem> b)
        {
            b.ToTable(TableName, Schema.Wfs)
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
            b.Property(p => p.Position).HasColumnType("sys.geometry");
            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);
            b.Property(p => p.VersionAsString);

            b.HasIndex(p => p.StreetNamePersistentLocalId);
            b.HasIndex(p => p.Removed);
        }
    }
}
