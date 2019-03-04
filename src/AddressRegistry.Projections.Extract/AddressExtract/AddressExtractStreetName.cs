namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class AddressExtractStreetName
    {
        public Guid StreetNameId { get; set; }
        public string OsloId { get; set; }
        public string NisCode { get; set; }

        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }

        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
    }

    public class AddressExtractStreetNameConfiguration : IEntityTypeConfiguration<AddressExtractStreetName>
    {
        private const string TableName = "AddressExtractStreetNames";

        public void Configure(EntityTypeBuilder<AddressExtractStreetName> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(x => x.StreetNameId)
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.OsloId);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);

            builder.HasIndex(x => x.NisCode);
        }
    }
}
