namespace AddressRegistry.Projections.Integration.Merger
{
    using AddressRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressMergerItem
    {
        public int NewPersistentLocalId { get; set; }
        public int MergedPersistentLocalId { get; set; }

        public AddressMergerItem()
        {  }

        public AddressMergerItem(int newPersistentLocalId, int mergedPersistentLocalId)
        {
            NewPersistentLocalId = newPersistentLocalId;
            MergedPersistentLocalId = mergedPersistentLocalId;
        }
    }
    public sealed class AddressMergerItemConfiguration : IEntityTypeConfiguration<AddressMergerItem>
    {
        internal const string TableName = "address_merger_items";

        public void Configure(EntityTypeBuilder<AddressMergerItem> builder)
        {
            builder.ToTable(TableName, Schema.Integration)
                .HasKey(x => new { x.NewPersistentLocalId, x.MergedPersistentLocalId})
                .IsClustered();

            builder.Property(x => x.NewPersistentLocalId).HasColumnName("new_persistent_local_id");
            builder.Property(x => x.MergedPersistentLocalId).HasColumnName("merged_persistent_local_id");

            builder.HasIndex(x => x.NewPersistentLocalId);
            builder.HasIndex(x => x.MergedPersistentLocalId);
        }
    }
}
