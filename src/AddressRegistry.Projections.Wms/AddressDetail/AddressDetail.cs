namespace AddressRegistry.Projections.Wms.AddressDetail
{
    using System;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class AddressDetailItem
    {
        public Guid AddressId { get; set; }
        public int? PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? HouseNumberLabel { get; private set; }
        public int? HouseNumberLabelLength { get; private set; }
        public WmsAddressLabelType LabelType { get; set; }

        public string? BoxNumber { get; set; }
        public string? Status { get; set; }
        public bool? OfficiallyAssigned { get; set; }
        public Point? Position { get; private set; }
        public double? PositionX { get; private set; }
        public double? PositionY { get; private set; }

        public string? PositionMethod { get; set; }
        public string? PositionSpecification { get; set; }

        public bool Complete { get; set; }
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

        public void SetHouseNumberLabel(string? label)
        {
            HouseNumberLabel = label;
            HouseNumberLabelLength = label?.Length;
        }

        public void SetPosition(Point? position)
        {
            Position = position;
            PositionX = position?.X;
            PositionY = position?.Y;
        }
    }

    public class AddressDetailItemConfiguration : IEntityTypeConfiguration<AddressDetailItem>
    {
        internal const string TableName = "AddressDetails";

        public void Configure(EntityTypeBuilder<AddressDetailItem> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.AddressId)
                .IsClustered();

            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNameId);
            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.OfficiallyAssigned);

            b.Property(p => p.Position)
                .HasColumnType("sys.geometry");

            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Complete);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);
            b.Property(p => p.VersionAsString);

            b.HasIndex(p => p.PersistentLocalId);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.StreetNameId);
            b.HasIndex(p => new  {p.PositionX, p.PositionY, p.Removed, p.Complete, p.Status} ).IncludeProperties(x => x.StreetNameId);
        }
    }
}
