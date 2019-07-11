namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelAddressMatchLatestItem
    {
        public Guid ParcelId { get; set; }
        public string ParcelPersistentLocalId { get; set; }
        public Guid AddressId { get; set; }
    }

    public class ParcelAddressLatestItemConfiguration : IEntityTypeConfiguration<ParcelAddressMatchLatestItem>
    {
        private const string TableName = "ParcelAddressMatchLatestItemSyndication";

        public void Configure(EntityTypeBuilder<ParcelAddressMatchLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new
                {
                    x.ParcelId,
                    x.AddressId
                })
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.ParcelPersistentLocalId);

            builder.HasIndex(x => x.AddressId);
        }
    }
}
