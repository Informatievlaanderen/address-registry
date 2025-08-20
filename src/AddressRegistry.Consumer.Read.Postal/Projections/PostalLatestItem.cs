namespace AddressRegistry.Consumer.Read.Postal.Projections
{
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class PostalLatestItem
    {
        public string PostalCode { get; set; }
        public PostalStatus Status { get; set; }
        public string? NisCode { get; set; }

        public virtual ICollection<PostalInfoPostalName> PostalNames { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public bool IsRemoved { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public PostalLatestItem()
        {
            NisCode = null;
            PostalNames = new List<PostalInfoPostalName>();
        }

        public PostalLatestItem(string postalCode, Instant timestamp)
            : this()
        {
            PostalCode = postalCode;
            VersionTimestamp = timestamp;
        }
    }

    public class PostalInfoPostalName
    {
        public string PostalCode { get; set; }
        public PostalLanguage Language { get; set; }
        public string? PostalName { get; set; }

        public PostalInfoPostalName(
            string postalCode,
            PostalLanguage language,
            string? postalName)
        {
            PostalCode = postalCode;
            Language = language;
            PostalName = postalName;
        }

        public PostalInfoPostalName()
        { }
    }

    public enum PostalStatus
    {
        Current = 0,
        Retired = 1,
    }

    public enum PostalLanguage
    {
        Dutch = 0,
        French = 1,
        English = 2,
        German = 3
    }

    public class PostalItemConfiguration : IEntityTypeConfiguration<PostalLatestItem>
    {
        public const string TableName = "LatestItems";

        public void Configure(EntityTypeBuilder<PostalLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadPostal)
                .HasKey(x => x.PostalCode)
                .IsClustered();

            builder.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            builder.Property(p => p.IsRemoved)
                .HasDefaultValue(false);

            builder.HasIndex(p => p.VersionTimestampAsDateTimeOffset);

            builder.Ignore(x => x.VersionTimestamp);

            builder.Property(x => x.NisCode);
            builder.Property(x => x.Status).HasConversion<string>();

            builder.HasMany(x => x.PostalNames)
                .WithOne()
                .HasForeignKey("PostalCode")
                .IsRequired();

            builder.HasIndex(x => x.NisCode);
        }
    }

    public class PostalNameLatestItemConfiguration : IEntityTypeConfiguration<PostalInfoPostalName>
    {
        private const string TableName = "LatestItemPostalNames";

        public void Configure(EntityTypeBuilder<PostalInfoPostalName> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadPostal)
                .HasKey(x => new { x.PostalCode, x.PostalName })
                .IsClustered(false);

            builder.Property(x => x.Language).HasConversion<string>();

            builder.HasIndex(x => x.PostalName);
        }
    }
}
