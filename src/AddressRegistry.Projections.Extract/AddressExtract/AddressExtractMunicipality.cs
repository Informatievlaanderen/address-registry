namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class AddressExtractMunicipality
    {
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }
        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }
        public Taal? PrimaryLanguage { get; set; }
        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
    }

    public class AddressExtractMunicipalityConfiguration : IEntityTypeConfiguration<AddressExtractMunicipality>
    {
        private const string TableName = "AddressExtractMunicipalities";

        public void Configure(EntityTypeBuilder<AddressExtractMunicipality> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(x => x.MunicipalityId)
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);
            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.PrimaryLanguage);

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.NisCode).ForSqlServerIsClustered();
        }
    }
}
