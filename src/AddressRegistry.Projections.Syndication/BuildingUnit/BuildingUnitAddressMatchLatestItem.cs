namespace AddressRegistry.Projections.Syndication.BuildingUnit
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitAddressMatchLatestItem
    {
        public Guid BuildingUnitId { get; set; }
        public string BuildingUnitOsloId { get; set; }
        public Guid AddressId { get; set; }
        public Guid BuildingId { get; set; }
        public bool IsComplete { get; set; }
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
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.BuildingUnitOsloId);
            builder.Property(x => x.BuildingId);
            builder.Property(x => x.IsComplete);

            builder.HasIndex(x => new { x.AddressId, x.IsComplete });
            builder.HasIndex(x => x.BuildingId);
        }
    }
}
