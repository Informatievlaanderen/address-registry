namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StreetName;

    public class AddressBoxNumberSyndicationHelper
    {
        public int PersistentLocalId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public byte[]? PointPosition { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public AddressStatus? Status { get; set; }
        public bool IsComplete { get; set; }
        public bool IsOfficiallyAssigned { get; set; }
    }

    public class AddressBoxNumberSyndicationHelperConfiguration : IEntityTypeConfiguration<AddressBoxNumberSyndicationHelper>
    {
        private const string TableName = "AddressBoxNumberSyndicationHelper";

        public void Configure(EntityTypeBuilder<AddressBoxNumberSyndicationHelper> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            b.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            b.Property(x => x.PostalCode);
            b.Property(x => x.HouseNumber);
            b.Property(x => x.BoxNumber);

            b.Property(x => x.Status);
            b.Property(x => x.IsOfficiallyAssigned);
            b.Property(x => x.IsComplete);

            b.Property(x => x.PointPosition);
            b.Property(x => x.PositionMethod);
            b.Property(x => x.PositionSpecification);
        }
    }
}
