namespace AddressRegistry.Projections.Wms.AddressWmsItemV3
{
    using System;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class AddressWmsItemV3
    {
        protected AddressWmsItemV3()
        { }

        public AddressWmsItemV3(
            int addressPersistentLocalId,
            int? parentAddressPersistentLocalId,
            int streetNamePersistentLocalId,
            string? postalCode,
            string houseNumber,
            string? boxNumber,
            string status,
            bool officiallyAssigned,
            Point position,
            string positionMethod,
            string positionSpecification,
            bool removed,
            Instant versionTimestamp)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            ParentAddressPersistentLocalId = parentAddressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            LabelType = string.IsNullOrWhiteSpace(boxNumber)
                ? WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition
                : WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition;
            Status = status;
            OfficiallyAssigned = officiallyAssigned;
            SetPosition(position);
            PositionMethod = positionMethod;
            PositionSpecification = positionSpecification;
            Removed = removed;
            VersionTimestamp = versionTimestamp;
        }

        public int AddressPersistentLocalId { get; set; }
        public int? ParentAddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public string? PostalCode { get; set; }
        public string HouseNumber { get; set; }
        public string? HouseNumberLabel { get; private set; }
        public int? HouseNumberLabelLength { get; private set; }
        public WmsAddressLabelType LabelType { get; set; }

        public string? BoxNumber { get; set; }
        public string Status { get; set; }
        public bool OfficiallyAssigned { get; set; }
        public Point Position { get; private set; }
        public double PositionX { get; private set; }
        public double PositionY { get; private set; }

        public string PositionMethod { get; set; }
        public string PositionSpecification { get; set; }

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

        public void SetHouseNumberLabel(string? label, WmsAddressLabelType labelType)
        {
            HouseNumberLabel = label;
            HouseNumberLabelLength = label?.Length;
            LabelType = labelType;
        }

        public void SetPosition(Point position)
        {
            Position = position;
            PositionX = position.X;
            PositionY = position.Y;
        }
    }

    public class AddressWmsItemV3Configuration : IEntityTypeConfiguration<AddressWmsItemV3>
    {
        internal const string TableName = "AddressWmsV3";

        public void Configure(EntityTypeBuilder<AddressWmsItemV3> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.AddressPersistentLocalId)
                .IsClustered();

            b.Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNamePersistentLocalId);
            b.Property(p => p.ParentAddressPersistentLocalId);
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

            b.HasIndex(p => p.ParentAddressPersistentLocalId);
            b.HasIndex(p => p.Status);
            b
                .HasIndex(p => new { p.Removed, p.Status })
                .IncludeProperties(x => new { x.StreetNamePersistentLocalId, x.Position });
            b.HasIndex(p => p.StreetNamePersistentLocalId);

            b.HasIndex(p => new { p.PositionX, p.PositionY, p.Removed, p.Status })
                .IncludeProperties(x => x.StreetNamePersistentLocalId);
        }
    }
}
