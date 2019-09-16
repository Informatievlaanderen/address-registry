namespace AddressRegistry.Projections.Syndication.StreetName
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StreetNameBosaItem
    {
        public Guid StreetNameId { get; set; }
        public string PersistentLocalId { get; set; }
        public string NisCode { get; set; }

        public string NameDutch { get; set; }
        public string NameDutchSearch { get; set; }
        public string NameFrench { get; set; }
        public string NameFrenchSearch { get; set; }
        public string NameGerman { get; set; }
        public string NameGermanSearch { get; set; }
        public string NameEnglish { get; set; }
        public string NameEnglishSearch { get; set; }

        public string HomonymAdditionDutch { get; set; }
        public string HomonymAdditionFrench { get; set; }
        public string HomonymAdditionGerman { get; set; }
        public string HomonymAdditionEnglish { get; set; }

        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
        public bool IsComplete { get; set; }
    }

    public class StreetNameBosaItemConfiguration : IEntityTypeConfiguration<StreetNameBosaItem>
    {
        private const string TableName = "StreetNameBosa";

        public void Configure(EntityTypeBuilder<StreetNameBosaItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.StreetNameId)
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.PersistentLocalId);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.NameDutchSearch);
            builder.Property(x => x.NameFrenchSearch);
            builder.Property(x => x.NameGermanSearch);
            builder.Property(x => x.NameEnglishSearch);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.IsComplete);

            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => x.IsComplete);
            builder.HasIndex(x => x.Version);

            builder.HasIndex(x => x.NameDutchSearch);
            builder.HasIndex(x => x.NameFrenchSearch);
            builder.HasIndex(x => x.NameGermanSearch);
            builder.HasIndex(x => x.NameEnglishSearch);
        }
    }
}
