namespace AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class CrabIdToPersistentLocalIdItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid AddressId { get; set; }
        public int? PersistentLocalId { get; set; }
        public int? HouseNumberId { get; set; }
        public int? SubaddressId { get; set; }
        public string HouseNumber { get; set; }
        public string BoxNumber { get; set; }
        public string PostalCode { get; set; }
        public Guid StreetNameId { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class CrabIdToPersistentLocalIdItemConfiguration : IEntityTypeConfiguration<CrabIdToPersistentLocalIdItem>
    {
        public const string TableName = "CrabIdToPersistentLocalIds";

        public void Configure(EntityTypeBuilder<CrabIdToPersistentLocalIdItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressId)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.PersistentLocalId);
            b.Property(p => p.HouseNumberId);
            b.Property(p => p.SubaddressId);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.PostalCode);
            b.Property(p => p.StreetNameId);
            b.Property(p => p.IsRemoved);
            b.Property(p => p.IsComplete);

            b.Property(CrabIdToPersistentLocalIdItem.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.HasIndex(p => p.PersistentLocalId).IsUnique();
            b.HasIndex(p => p.HouseNumberId);
            b.HasIndex(p => p.SubaddressId);
            b.HasIndex(p => p.IsRemoved);
        }
    }
}
