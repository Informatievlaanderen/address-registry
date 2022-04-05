namespace AddressRegistry.Projections.Wms.AddressDetail
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Address.ValueObjects;
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
        public WmsAddressLabelType LabelType { get; set; }

        public string? BoxNumber { get; set; }
        public string? Status { get; set; }
        public bool? OfficiallyAssigned { get; set; }
        public Point? Position { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? PositionAsText { get; }
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

            b.Property(p => p.PositionAsText)
                .HasComputedColumnSql("[Position].STAsText()", stored: true);

            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Complete);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);
            b.Property(p => p.VersionAsString);

            b.HasIndex(p => p.PersistentLocalId);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.StreetNameId);
            b.HasIndex(p => new  {p.Removed, p.Complete} );
        }
    }
}
