namespace AddressRegistry.Projections.Legacy.AddressList
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;

    public class AddressListItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid AddressId { get; set; }
        public int PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string PostalCode { get; set; }
        public string HouseNumber { get; set; }
        public string BoxNumber { get; set; }

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
        public const string TableName = "AddressList";

        public void Configure(EntityTypeBuilder<AddressListItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressId)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.StreetNameId);

            b.Property(AddressListItem.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.Complete);
            b.Property(p => p.Removed);

            b.HasIndex(p => p.BoxNumber);
            b.HasIndex(p => p.HouseNumber);
            b.HasIndex(p => p.PostalCode);
            b.HasIndex(p => new { p.Complete, p.Removed });
        }
    }
}
