namespace AddressRegistry.Projections.Syndication.AddressLink
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressLinkSyndicationItem
    {
        public Guid AddressId { get; set; }
        public string? PersistentLocalId { get; set; }
        public string? Version { get; set; }
        public long Position { get; set; }

        public Guid? StreetNameId { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public string? PostalCode { get; set; }

        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
    }

    public class AddressPersistentLocalIdItemConfiguration : IEntityTypeConfiguration<AddressLinkSyndicationItem>
    {
        private const string TableName = "AddressLinksExtract_Addresses";

        public void Configure(EntityTypeBuilder<AddressLinkSyndicationItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.AddressId)
                .IsClustered();

            builder.Property(x => x.PersistentLocalId);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.IsComplete);
            builder.Property(x => x.IsRemoved);
        }
    }

}
