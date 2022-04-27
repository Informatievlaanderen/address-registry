namespace AddressRegistry.Projections.Legacy.AddressList
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;
    using Address;

    public class AddressListItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid AddressId { get; set; }
        public int PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public AddressStatus? Status { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class AddressListItemConfiguration : IEntityTypeConfiguration<AddressListItem>
    {
        internal const string TableName = "AddressList";

        public void Configure(EntityTypeBuilder<AddressListItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressId)
                .IsClustered(false);

            b.Property(p => p.StreetNameId);

            b.Property(AddressListItem.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.Status);
            b.Property(p => p.Complete);
            b.Property(p => p.Removed);

            b.HasIndex(p => p.BoxNumber);
            b.HasIndex(p => p.HouseNumber);
            b.HasIndex(p => p.PostalCode);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => new { p.Complete, p.Removed });
            b.HasIndex(p => new { p.Complete, p.Removed, p.PersistentLocalId }).IncludeProperties(p => p.StreetNameId);
            b.HasIndex(p => p.StreetNameId);

            b.HasIndex(p => p.PersistentLocalId)
                .IsClustered();

            b.HasIndex(p => p.PersistentLocalId)
                .IsUnique()
                .IsClustered(false)
                .HasFilter($"([{nameof(AddressListItem.PersistentLocalId)}] IS NOT NULL)")
                .HasDatabaseName("IX_AddressList_PersistentLocalId_1");
        }
    }
}
