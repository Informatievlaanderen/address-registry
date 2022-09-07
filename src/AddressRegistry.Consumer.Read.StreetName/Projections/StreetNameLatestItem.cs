namespace AddressRegistry.Consumer.Read.StreetName.Projections
{
    using System;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using NodaTime.Text;

    public class StreetNameLatestItem
    {
        public int PersistentLocalId { get; set; }
        public string? NisCode { get; set; }
        public StreetNameStatus? Status { get; set; }

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

        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public StreetNameLatestItem()
        { }

        public StreetNameLatestItem(
            int streetNamePersistentLocalId,
            string niscode)
        {
            PersistentLocalId = streetNamePersistentLocalId;
            NisCode = niscode;
        }

        public bool HasHomonymAddition =>
            !string.IsNullOrEmpty(HomonymAdditionDutch) ||
            !string.IsNullOrEmpty(HomonymAdditionEnglish) ||
            !string.IsNullOrEmpty(HomonymAdditionFrench) ||
            !string.IsNullOrEmpty(HomonymAdditionGerman);

        public string GetDefaultName(Taal? primaryLanguageStreetName)
            => primaryLanguageStreetName switch
            {
                Taal.NL => NameDutch,
                Taal.FR => NameFrench,
                Taal.DE => NameGerman,
                Taal.EN => NameEnglish,
                _ => NameDutch
            };

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public static StreetNameStatus ConvertStringToStatus(string status)
            => Enum.Parse<StreetNameStatus>(status);
    }

    public enum StreetNameStatus
    {
        Proposed = 0,
        Current = 1,
        Retired = 2,
        Rejected = 3
    }

    public enum StreetNameLanguage
    {
        Dutch = 0,
        French = 1,
        English = 2,
        German = 3
    }

    public class StreetNameLatestItemConfiguration : IEntityTypeConfiguration<StreetNameLatestItem>
    {
        private const string TableName = "StreetNameLatestItem";

        public void Configure(EntityTypeBuilder<StreetNameLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadStreetName)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            builder.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(x => x.VersionTimestamp);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.Status);

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

            builder.Property(x => x.IsRemoved);

            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.IsRemoved);

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
