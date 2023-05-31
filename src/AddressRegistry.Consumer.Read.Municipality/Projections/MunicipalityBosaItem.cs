namespace AddressRegistry.Consumer.Read.Municipality.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public class MunicipalityBosaItem
    {
        public static string OfficialLanguagesBackingPropertyName => nameof(OfficialLanguagesAsString);

        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }
        public string? NameDutch { get; set; }
        public string? NameDutchSearch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameFrenchSearch { get; set; }
        public string? NameGerman { get; set; }
        public string? NameGermanSearch { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameEnglishSearch { get; set; }

        public byte[]? ExtendedWkbGeometry { get; set; }

        public bool IsFlemishRegion { get; set; }


        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        private string OfficialLanguagesAsString { get; set; } = "";

        public List<string> OfficialLanguages
        {
            get => DeserializeOfficialLanguages();
            set => OfficialLanguagesAsString = JsonConvert.SerializeObject(value);
        }

        private List<string> DeserializeOfficialLanguages()
        {
            return string.IsNullOrEmpty(OfficialLanguagesAsString)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(OfficialLanguagesAsString) ?? new List<string>();
        }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public MunicipalityLanguage PrimaryLanguage
        {
            get
            {
                var dictionaryLanguages = new Dictionary<string, MunicipalityLanguage>(new List<KeyValuePair<string, MunicipalityLanguage>>(), StringComparer.OrdinalIgnoreCase)
                {
                    { "Dutch", MunicipalityLanguage.Dutch },
                    { "French", MunicipalityLanguage.French },
                    { "English", MunicipalityLanguage.English },
                    { "German", MunicipalityLanguage.German }
                };

                return dictionaryLanguages[OfficialLanguages.First()];
            }
        }

        public MunicipalityBosaItem()
        {
            NisCode = string.Empty;
            OfficialLanguages = new List<string>();
        }

        public MunicipalityBosaItem(Guid municipalityId, string nisCode, Instant timestamp, bool isFlemishRegion)
            : this()
        {
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            VersionTimestamp = timestamp;
            IsFlemishRegion = isFlemishRegion;
        }
    }

    public class MunicipalityBosaItemConfiguration : IEntityTypeConfiguration<MunicipalityBosaItem>
    {
        private const string TableName = "BosaItems";

        public void Configure(EntityTypeBuilder<MunicipalityBosaItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadMunicipality)
                .HasKey(x => x.MunicipalityId)
                .IsClustered(false);

            builder.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(x => x.VersionTimestamp);

            builder.Ignore(x => x.OfficialLanguages);
            builder.Property(MunicipalityLatestItem.OfficialLanguagesBackingPropertyName)
                .HasColumnName("OfficialLanguages");

            builder.Property(x => x.NisCode);
            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameDutchSearch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameFrenchSearch);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameGermanSearch);
            builder.Property(x => x.NameEnglish);
            builder.Property(x => x.NameEnglishSearch);

            builder.HasIndex(x => x.NisCode).IsClustered();
            builder.HasIndex(x => x.VersionTimestampAsDateTimeOffset);
            builder.HasIndex(x => x.IsFlemishRegion);
            builder.HasIndex(x => x.NameDutchSearch);
            builder.HasIndex(x => x.NameFrenchSearch);
            builder.HasIndex(x => x.NameEnglishSearch);
            builder.HasIndex(x => x.NameGermanSearch);
        }
    }
}