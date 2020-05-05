namespace AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressCrabHouseNumberIdExtractItem
    {
        public Guid? AddressId { get; set; }
        public int? PersistentLocalId { get; set; }
        public int? CrabHouseNumberId { get; set; }
        public byte[]? DbaseRecord { get; set; }
    }

    public class AddressCrabHouseNumberIdExtractItemConfiguration : IEntityTypeConfiguration<AddressCrabHouseNumberIdExtractItem>
    {
        private const string TableName = "AddressIdCrabHouseNumberId";

        public void Configure(EntityTypeBuilder<AddressCrabHouseNumberIdExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.AddressId)
                .IsClustered(false);

            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.PersistentLocalId);
            builder.Property(p => p.CrabHouseNumberId);
        }
    }
}
