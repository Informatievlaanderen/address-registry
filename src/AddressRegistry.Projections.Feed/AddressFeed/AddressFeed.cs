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

        public int PersistentLocalId { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private AddressFeedItem() { }

        public AddressFeedItem(long position, int page, int persistentLocalId) : this()
        {
            PersistentLocalId = persistentLocalId;
            Page = page;
            Position = position;
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

            b.Property(x => x.PersistentLocalId).IsRequired();

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.Page);
            b.HasIndex(x => x.PersistentLocalId);
        }
    }
}
