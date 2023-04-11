namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressExtractItemV2
    {
        public int AddressPersistentLocalId { get; set; }
        public byte[]? DbaseRecord { get; set; }
        public byte[]? ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
    }

    public class AddressExtractItemV2Configuration : IEntityTypeConfiguration<AddressExtractItemV2>
    {
        private const string TableName = "AddressV2";

        public void Configure(EntityTypeBuilder<AddressExtractItemV2> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.AddressPersistentLocalId)
                .IsClustered(true);

            builder.Property(p => p.AddressPersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.ShapeRecordContent);
            builder.Property(p => p.ShapeRecordContentLength);
            builder.Property(p => p.MaximumX);
            builder.Property(p => p.MinimumX);
            builder.Property(p => p.MinimumY);
            builder.Property(p => p.MaximumY);
            builder.Property(p => p.StreetNamePersistentLocalId);

            builder.HasIndex(p => p.StreetNamePersistentLocalId);
        }
    }
}
