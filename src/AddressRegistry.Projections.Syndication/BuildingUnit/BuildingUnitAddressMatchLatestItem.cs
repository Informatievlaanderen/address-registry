namespace AddressRegistry.Projections.Syndication.BuildingUnit
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitAddressMatchLatestItem
    {
        public Guid BuildingUnitId { get; set; }
        public string? BuildingUnitPersistentLocalId { get; set; }
        public Guid AddressId { get; set; }
        public Guid BuildingId { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
    }

    public class ParcelAddressLatestItemConfiguration : IEntityTypeConfiguration<BuildingUnitAddressMatchLatestItem>
    {
        private const string TableName = "BuildingUnitAddressMatchLatestItemSyndication";

        public void Configure(EntityTypeBuilder<BuildingUnitAddressMatchLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new
                {
                    x.BuildingUnitId,
                    x.AddressId
                })
                .IsClustered(false);

            builder.Property(x => x.BuildingUnitPersistentLocalId);
            builder.Property(x => x.BuildingId);
            builder.Property(x => x.IsComplete);
            builder.Property(x => x.IsRemoved);

            builder.HasIndex(x => x.AddressId);
            builder.HasIndex(x => new { x.IsComplete, x.IsRemoved });
            builder.HasIndex(x => x.BuildingId);
        }
    }
}
