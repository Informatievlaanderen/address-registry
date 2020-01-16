namespace AddressRegistry.Projections.Syndication.Municipality
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MunicipalityBosaItem
    {
        public Guid MunicipalityId { get; set; }
        public string? NisCode { get; set; }
        public string? NameDutch { get; set; }
        public string? NameDutchSearch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameFrenchSearch { get; set; }
        public string? NameGerman { get; set; }
        public string? NameGermanSearch { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameEnglishSearch { get; set; }
        public bool IsFlemishRegion { get; set; }

        public Taal? PrimaryLanguage { get; set; }
        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
    }

    public class MunicipalityBosaItemConfiguration : IEntityTypeConfiguration<MunicipalityBosaItem>
    {
        private const string TableName = "MunicipalityBosa";

        public void Configure(EntityTypeBuilder<MunicipalityBosaItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.MunicipalityId)
                .IsClustered(false);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameDutchSearch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameFrenchSearch);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameGermanSearch);
            builder.Property(x => x.NameEnglish);
            builder.Property(x => x.NameEnglishSearch);
            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.PrimaryLanguage);
            builder.Property(x => x.IsFlemishRegion);

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.NisCode).IsClustered();
            builder.HasIndex(x => x.IsFlemishRegion);

            builder.HasIndex(x => x.Version);

            builder.HasIndex(x => x.NameDutchSearch);
            builder.HasIndex(x => x.NameFrenchSearch);
            builder.HasIndex(x => x.NameEnglishSearch);
            builder.HasIndex(x => x.NameGermanSearch);
        }
    }
}
