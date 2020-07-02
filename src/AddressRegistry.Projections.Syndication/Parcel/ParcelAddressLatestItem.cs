namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelAddressMatchLatestItem
    {
        public Guid ParcelId { get; set; }
        public string? ParcelPersistentLocalId { get; set; }
        public Guid AddressId { get; set; }
        public bool IsRemoved { get; set; }
    }

    public class ParcelAddressLatestItemConfiguration : IEntityTypeConfiguration<ParcelAddressMatchLatestItem>
    {
        public const string TableName = "ParcelAddressMatchLatestItemSyndication";

        public void Configure(EntityTypeBuilder<ParcelAddressMatchLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new
                {
                    x.ParcelId,
                    x.AddressId
                })
                .IsClustered(false);

            builder.Property(x => x.ParcelPersistentLocalId);
            builder.Property(x => x.IsRemoved);

            builder.HasIndex(x => x.ParcelId);
            builder.HasIndex(x => x.AddressId).IsClustered();
            builder.HasIndex(x => x.IsRemoved)
                .IncludeProperties(p => new { p.AddressId, p.ParcelId });
        }
    }
}
