namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressParcelLinkExtractItem
    {
        public Guid AddressId { get; set; }
        public Guid ParcelId { get; set; }
        public string? AddressPersistentLocalId { get; set; }
        public string? ParcelPersistentLocalId { get; set; }
        public byte[]? DbaseRecord { get; set; }
        public bool IsAddressLinkRemoved { get; set; }
        public bool IsParcelRemoved { get; set; }
    }

    public class AddressParcelLinkExtractItemConfiguration : IEntityTypeConfiguration<AddressParcelLinkExtractItem>
    {
        public const string TableName = "AddressParcelLinksExtract";

        public void Configure(EntityTypeBuilder<AddressParcelLinkExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(p => new { p.AddressId, p.ParcelId })
                .IsClustered(false);

            builder.Property(p => p.AddressId);
            builder.Property(p => p.ParcelId);
            builder.Property(p => p.AddressPersistentLocalId);
            builder.Property(p => p.ParcelPersistentLocalId);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.IsParcelRemoved);
            builder.Property(p => p.IsAddressLinkRemoved);

            builder.HasIndex(p => p.AddressId);
            builder.HasIndex(p => p.ParcelId);
            builder.HasIndex(p => p.AddressPersistentLocalId).IsClustered();
            builder.HasIndex(p => new { p.IsParcelRemoved, p.IsAddressLinkRemoved })
                .IncludeProperties(p => new { p.AddressId, p.ParcelId });
        }
    }
}
