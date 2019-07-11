namespace AddressRegistry.Projections.Syndication.StreetName
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StreetNameSyndicationItem
    {
        public Guid StreetNameId { get; set; }
        public string PersistentLocalId { get; set; }
        public string NisCode { get; set; }

        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }

        public string HomonymAdditionDutch { get; set; }
        public string HomonymAdditionFrench { get; set; }
        public string HomonymAdditionGerman { get; set; }
        public string HomonymAdditionEnglish { get; set; }

        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
    }

    public class StreetNameSyndicationItemConfiguration : IEntityTypeConfiguration<StreetNameSyndicationItem>
    {
        private const string TableName = "StreetNameSyndication";

        public void Configure(EntityTypeBuilder<StreetNameSyndicationItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new { x.StreetNameId, x.Position })
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.PersistentLocalId);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Version);

            builder.HasIndex(x => x.NameDutch);
            builder.HasIndex(x => x.NameFrench);
            builder.HasIndex(x => x.NameEnglish);
            builder.HasIndex(x => x.NameGerman);

            builder.HasIndex(x => x.HomonymAdditionDutch);
            builder.HasIndex(x => x.HomonymAdditionFrench);
            builder.HasIndex(x => x.HomonymAdditionGerman);
            builder.HasIndex(x => x.HomonymAdditionEnglish);
        }
    }
}
