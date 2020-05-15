namespace AddressRegistry.Projections.Syndication.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StreetNameLatestItem
    {
        public Guid StreetNameId { get; set; }
        public string? PersistentLocalId { get; set; }
        public string? NisCode { get; set; }

        public string? NameDutch { get; set; }
        public string? NameDutchSearch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameFrenchSearch { get; set; }
        public string? NameGerman { get; set; }
        public string? NameGermanSearch { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameEnglishSearch { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public string? Version { get; set; }
        public long Position { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }

        public bool HasHomonymAddition =>
            !string.IsNullOrEmpty(HomonymAdditionDutch) ||
            !string.IsNullOrEmpty(HomonymAdditionEnglish) ||
            !string.IsNullOrEmpty(HomonymAdditionFrench) ||
            !string.IsNullOrEmpty(HomonymAdditionGerman);

        public string GetDefaultName(Taal? primaryLanguageMunicipality)
        {
            switch (primaryLanguageMunicipality)
            {
                default:
                case Taal.NL:
                    return NameDutch;
                case Taal.FR:
                    return NameFrench;
                case Taal.DE:
                    return NameGerman;
                case Taal.EN:
                    return NameEnglish;
            }
        }
    }

    public class StreetNameLatestItemConfiguration : IEntityTypeConfiguration<StreetNameLatestItem>
    {
        private const string TableName = "StreetNameLatestSyndication";

        public void Configure(EntityTypeBuilder<StreetNameLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.StreetNameId)
                .IsClustered();

            builder.Property(x => x.NisCode);
            builder.Property(x => x.PersistentLocalId);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameDutchSearch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameFrenchSearch);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameGermanSearch);
            builder.Property(x => x.NameEnglish);
            builder.Property(x => x.NameEnglishSearch);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.IsComplete);
            builder.Property(x => x.IsRemoved);

            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => new { x.IsComplete, x.IsRemoved });

            builder.HasIndex(x => x.NameDutch);
            builder.HasIndex(x => x.NameFrench);
            builder.HasIndex(x => x.NameEnglish);
            builder.HasIndex(x => x.NameGerman);

            builder.HasIndex(x => x.NameDutchSearch);
            builder.HasIndex(x => x.NameFrenchSearch);
            builder.HasIndex(x => x.NameEnglishSearch);
            builder.HasIndex(x => x.NameGermanSearch);

            builder.HasIndex(x => x.HomonymAdditionDutch);
            builder.HasIndex(x => x.HomonymAdditionFrench);
            builder.HasIndex(x => x.HomonymAdditionGerman);
            builder.HasIndex(x => x.HomonymAdditionEnglish);
        }
    }
}
