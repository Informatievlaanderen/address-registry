namespace AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressCrabSubaddressIdExtractItem
    {
        public Guid? AddressId { get; set; }
        public int? PersistentLocalId { get; set; }
        public int? CrabSubaddressId { get; set; }
        public byte[]? DbaseRecord { get; set; }
    }

    public class AddressCrabSubaddressIdExtractItemConfiguration : IEntityTypeConfiguration<AddressCrabSubaddressIdExtractItem>
    {
        private const string TableName = "AddressIdCrabSubaddressId";

        public void Configure(EntityTypeBuilder<AddressCrabSubaddressIdExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.AddressId)
                .IsClustered(false);

            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.PersistentLocalId);
            builder.Property(p => p.CrabSubaddressId);
        }
    }
}
