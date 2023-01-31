namespace AddressRegistry.Consumer.StreetName
{
    using System;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using AddressRegistry.Infrastructure;

    public class StreetNameConsumerItem
    {
        public Guid StreetNameId { get; set; }
        public int PersistentLocalId { get; set; }
    }

    public class StreetNameConsumerItemConfiguration : IEntityTypeConfiguration<StreetNameConsumerItem>
    {
        private const string TableName = "StreetNameConsumer";

        public void Configure(EntityTypeBuilder<StreetNameConsumerItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerProjections)
                .HasKey(x => x.StreetNameId)
                .IsClustered(false);

            builder.Property(x => x.PersistentLocalId);

            builder.HasIndex(x => x.PersistentLocalId).IsClustered();
        }
    }
}
