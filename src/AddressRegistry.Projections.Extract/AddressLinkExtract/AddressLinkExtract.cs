namespace AddressRegistry.Projections.Extract.AddressLinkExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressLinkExtractItem
    {
        public Guid AddressId { get; set; }
        public int PersistentLocalId { get; set; }
        public byte[] DbaseRecord { get; set; }
        public bool Complete { get; set; }
        public string HouseNumber { get; set; }
        public string BoxNumber { get; set; }
        public string PostalCode { get; set; }
        public Guid StreetNameId { get; set; }
    }

    public class AddressLinkExtractItemConfiguration : IEntityTypeConfiguration<AddressLinkExtractItem>
    {
        private const string TableName = "AddressLinks";

        public void Configure(EntityTypeBuilder<AddressLinkExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.AddressId)
                .ForSqlServerIsClustered(false);

            builder.Property(p => p.AddressId);
            builder.Property(p => p.PersistentLocalId);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.StreetNameId);
            builder.Property(p => p.HouseNumber);
            builder.Property(p => p.BoxNumber);
            builder.Property(p => p.PostalCode);

            builder.HasIndex(p => p.PersistentLocalId);
        }
    }
}
