namespace AddressRegistry.Projections.Syndication.BuildingUnit
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressBuildingUnitLinkExtractItem
    {
        public Guid AddressId { get; set; }
        public Guid BuildingUnitId { get; set; }
        public Guid BuildingId { get; set; }
        public string? AddressPersistentLocalId { get; set; }
        public string? BuildingUnitPersistentLocalId { get; set; }
        public byte[]? DbaseRecord { get; set; }
        public bool IsAddressLinkRemoved { get; set; }
        public bool IsBuildingUnitComplete { get; set; }
        public bool IsBuildingUnitRemoved { get; set; }
        public bool IsBuildingComplete { get; set; }
    }

    public class AddressBuildingUnitLinkExtractItemConfiguration : IEntityTypeConfiguration<AddressBuildingUnitLinkExtractItem>
    {
        private const string TableName = "AddressBuildingUnitLinksExtract";

        public void Configure(EntityTypeBuilder<AddressBuildingUnitLinkExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(p => new { p.AddressId, p.BuildingUnitId })
                .IsClustered(false);

            builder.Property(p => p.AddressId);
            builder.Property(p => p.BuildingUnitId);
            builder.Property(p => p.BuildingId);
            builder.Property(p => p.AddressPersistentLocalId);
            builder.Property(p => p.BuildingUnitPersistentLocalId);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.IsAddressLinkRemoved);
            builder.Property(p => p.IsBuildingUnitComplete);
            builder.Property(p => p.IsBuildingUnitRemoved);
            builder.Property(p => p.IsBuildingComplete);

            builder.HasIndex(p => p.AddressId);
            builder.HasIndex(p => p.BuildingUnitId);
            builder.HasIndex(p => p.BuildingId);

            builder.HasIndex(p => new { p.IsAddressLinkRemoved, p.IsBuildingUnitComplete, p.IsBuildingUnitRemoved, p.IsBuildingComplete })
                .IncludeProperties(x => new { x.AddressId, x.BuildingUnitId, x.BuildingUnitPersistentLocalId });

            builder.HasIndex(p => p.AddressPersistentLocalId).IsClustered();
        }
    }
}
