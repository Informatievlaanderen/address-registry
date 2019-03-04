namespace AddressRegistry.Projections.Legacy.CrabIdToOsloId
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class CrabIdToOsloIdItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid AddressId { get; set; }
        public int? OsloId { get; set; }
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

    public class CrabIdToOsloIdItemConfiguration : IEntityTypeConfiguration<CrabIdToOsloIdItem>
    {
        public const string TableName = "CrabIdToOsloIds";

        public void Configure(EntityTypeBuilder<CrabIdToOsloIdItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressId)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.OsloId);
            b.Property(p => p.HouseNumberId);
            b.Property(p => p.SubaddressId);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.PostalCode);
            b.Property(p => p.StreetNameId);
            b.Property(p => p.IsRemoved);
            b.Property(p => p.IsComplete);

            b.Property(CrabIdToOsloIdItem.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.HasIndex(p => p.OsloId).IsUnique();
            b.HasIndex(p => p.HouseNumberId);
            b.HasIndex(p => p.SubaddressId);
            b.HasIndex(p => p.IsRemoved);
        }
    }
}
