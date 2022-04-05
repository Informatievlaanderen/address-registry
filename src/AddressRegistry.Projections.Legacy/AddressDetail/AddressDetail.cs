namespace AddressRegistry.Projections.Legacy.AddressDetail
{
    using System;
    using Address.ValueObjects;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class AddressDetailItem
    {
        public Guid AddressId { get; set; }
        public int? PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public AddressStatus? Status { get; set; }
        public bool? OfficiallyAssigned { get; set; }

        public byte[]? Position { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class AddressDetailItemConfiguration : IEntityTypeConfiguration<AddressDetailItem>
    {
        internal const string TableName = "AddressDetails";

        public void Configure(EntityTypeBuilder<AddressDetailItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressId)
                .IsClustered(false);

            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNameId);
            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.OfficiallyAssigned);
            b.Property(p => p.Position);
            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Complete);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);

            b.HasIndex(p => p.PersistentLocalId)
                .IsUnique()
                .HasFilter($"([{nameof(AddressDetailItem.PersistentLocalId)}] IS NOT NULL)")
                .HasDatabaseName("IX_AddressDetails_PersistentLocalId_1");;

            b.HasIndex(p => p.PersistentLocalId)
                .IsClustered();

            // This speeds up AddressBosaQuery's huge StreetNameId IN (...) AND Complete = 1 query
            b.HasIndex(p => new { p.StreetNameId, p.Complete });
            b.HasIndex(p => p.HouseNumber);
            b.HasIndex(p => p.BoxNumber);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.PostalCode);
            b.HasIndex(p => p.VersionTimestampAsDateTimeOffset);

            b.HasIndex(p => new { p.Removed, p.Complete });
        }
    }
}
