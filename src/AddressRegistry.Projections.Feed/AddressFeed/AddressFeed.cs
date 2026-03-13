namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressFeedItem
    {
        public long Id { get; set; }
        public int Page { get; set; }
        public long Position { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private AddressFeedItem() { }

        public AddressFeedItem(long position, int page) : this()
        {
            Page = page;
            Position = position;
        }
    }

    public class AddressFeedItemAddress
    {
        public long FeedItemId { get; set; }
        public int AddressPersistentLocalId { get; set; }

        private AddressFeedItemAddress() { }

        public AddressFeedItemAddress(long feedItemId, int addressPersistentLocalId) : this()
        {
            FeedItemId = feedItemId;
            AddressPersistentLocalId = addressPersistentLocalId;
        }
    }

    public class AddressFeedConfiguration : IEntityTypeConfiguration<AddressFeedItem>
    {
        private const string TableName = "AddressFeed";

        public void Configure(EntityTypeBuilder<AddressFeedItem> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.Id)
                .IsClustered();

            b.Property(x => x.Id)
                .UseHiLo("AddressFeedSequence", Schema.Feed);

            b.Property(x => x.CloudEventAsString)
                .HasColumnName("CloudEvent")
                .IsRequired();

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.Page);
        }
    }

    public class AddressFeedItemAddressConfiguration : IEntityTypeConfiguration<AddressFeedItemAddress>
    {
        private const string TableName = "AddressFeedItemAddresses";

        public void Configure(EntityTypeBuilder<AddressFeedItemAddress> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => new { x.FeedItemId, x.AddressPersistentLocalId });

            b.HasIndex(x => x.FeedItemId);
            b.HasIndex(x => x.AddressPersistentLocalId);
        }
    }
}
