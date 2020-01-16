namespace AddressRegistry.Projections.Syndication.Municipality
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;

    public class MunicipalitySyndicationItem
    {
        public static string OfficialLanguagesBackingPropertyName = nameof(OfficialLanguagesAsString);

        public Guid MunicipalityId { get; set; }
        public string? NisCode { get; set; }
        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }
        public DateTimeOffset? Version { get; set; }

        private string? OfficialLanguagesAsString { get; set; }
        public List<Taal> OfficialLanguages
        {
            get => DeserializeOfficialLanguages();
            set => OfficialLanguagesAsString = JsonConvert.SerializeObject(value);
        }

        private List<Taal> DeserializeOfficialLanguages()
        {
            return string.IsNullOrEmpty(OfficialLanguagesAsString) ? new List<Taal>() : JsonConvert.DeserializeObject<List<Taal>>(OfficialLanguagesAsString);
        }

        public long Position { get; set; }
    }

    public class MunicipalityItemConfiguration : IEntityTypeConfiguration<MunicipalitySyndicationItem>
    {
        private const string TableName = "MunicipalitySyndication";

        public void Configure(EntityTypeBuilder<MunicipalitySyndicationItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new { x.MunicipalityId, x.Position })
                .IsClustered(false);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);
            builder.Property(x => x.Version);
            builder.Property(x => x.Position);

            builder.Ignore(x => x.OfficialLanguages);
            builder.Property(MunicipalitySyndicationItem.OfficialLanguagesBackingPropertyName)
                .HasColumnName("OfficialLanguages");

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.Version);
            builder.HasIndex(x => x.NisCode).IsClustered();
        }
    }
}
