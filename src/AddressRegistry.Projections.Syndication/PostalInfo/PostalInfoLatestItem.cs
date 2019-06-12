namespace AddressRegistry.Projections.Syndication.PostalInfo
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;
    using System.Collections.Generic;

    public class PostalInfoLatestItem
    {
        public string PostalCode { get; set; }

        public virtual ICollection<PostalInfoPostalName> PostalNames { get; set; }
        public string NisCode { get; set; }

        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }

        public PostalInfoLatestItem()
        {
            PostalNames = new List<PostalInfoPostalName>();
        }
    }

    public class PostalInfoPostalName
    {
        public string PostalCode { get; set; }
        public Taal Language { get; set; }
        public string PostalName { get; set; }
    }

    public class PostalInfoLatestItemConfiguration : IEntityTypeConfiguration<PostalInfoLatestItem>
    {
        private const string TableName = "PostalInfoLatestSyndication";

        public void Configure(EntityTypeBuilder<PostalInfoLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.PostalCode)
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.NisCode);

            builder.HasMany(x => x.PostalNames)
                .WithOne()
                .HasForeignKey("PostalCode")
                .IsRequired();

            builder.HasIndex(p => p.NisCode);
        }
    }

    public class PostalInfoPostalNameLatestItemConfiguration : IEntityTypeConfiguration<PostalInfoPostalName>
    {
        private const string TableName = "PostalInfoPostalNamesLatestSyndication";

        public void Configure(EntityTypeBuilder<PostalInfoPostalName> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new { x.PostalCode, x.PostalName })
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.Language);

            builder.HasIndex(x => x.PostalName);
        }
    }
}

