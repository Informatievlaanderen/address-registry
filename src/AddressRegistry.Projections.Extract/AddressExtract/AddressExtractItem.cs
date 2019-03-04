namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressExtractItem
    {
        public Guid? AddressId { get; set; }
        public int AddressOsloId { get; set; }
        public bool Complete { get; set; }
        public byte[] DbaseRecord { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }
        public Guid StreetNameId { get; set; }
        public string NisCode{ get; set; }
    }

    public class AddressExtractItemConfiguration : IEntityTypeConfiguration<AddressExtractItem>
    {
        public const string TableName = "Address";

        public void Configure(EntityTypeBuilder<AddressExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.AddressId)
                .ForSqlServerIsClustered(false);

            builder.Property(p => p.Complete);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.ShapeRecordContent);
            builder.Property(p => p.ShapeRecordContentLength);
            builder.Property(p => p.MaximumX);
            builder.Property(p => p.MinimumX);
            builder.Property(p => p.MinimumY);
            builder.Property(p => p.MaximumY);
            builder.Property(p => p.StreetNameId);
            builder.Property(p => p.NisCode);

            builder.HasIndex(p => p.StreetNameId);
            builder.HasIndex(p => p.NisCode);
        }
    }
}
