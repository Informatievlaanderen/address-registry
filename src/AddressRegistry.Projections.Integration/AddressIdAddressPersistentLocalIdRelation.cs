namespace AddressRegistry.Projections.Integration
{
    using System;
    using AddressRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressIdAddressPersistentLocalIdRelation
    {
        public Guid AddressId { get; set; }

        public int PersistentLocalId { get; set; }

        public AddressIdAddressPersistentLocalIdRelation()
        { }
    }

    public sealed class AddressIdAddressPersistentLocalIdConfiguration : IEntityTypeConfiguration<AddressIdAddressPersistentLocalIdRelation>
    {
        public void Configure(EntityTypeBuilder<AddressIdAddressPersistentLocalIdRelation> builder)
        {
            const string tableName = "address_id_address_persistent_local_id";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new { x.AddressId });


            builder.Property(x => x.AddressId).HasColumnName("address_id");
            builder.Property(x => x.PersistentLocalId).HasColumnName("persistent_local_id");

            builder.HasIndex(x => x.PersistentLocalId);
            builder.HasIndex(x => x.AddressId);
        }
    }
}
