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
        public bool AddressComplete { get; set; }
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
            builder.Property(p => p.AddressComplete);

            builder.HasIndex(p => p.AddressId);
            builder.HasIndex(p => p.BuildingUnitId);
            builder.HasIndex(p => p.BuildingId);
            builder.HasIndex(p => p.AddressPersistentLocalId).IsClustered();
        }
    }
}
