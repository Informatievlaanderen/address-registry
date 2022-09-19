namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StreetName;

    public class AddressBoxNumberSyndicationItem
    {
        //[Obsolete("Guid identifiers are no longer used.")]
        //public Guid? AddressId { get; set; }
        //[Obsolete("Guid identifiers are no longer used.")]
        //public Guid? StreetNameId { get; set; }


        //public int? PersistentLocalId { get; set; }
        public int PersistentLocalId { get; set; }
        //public int? StreetNamePersistentLocalId { get; set; }
        //public long Position { get; set; }
        //public string? ChangeType { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public byte[]? PointPosition { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public AddressStatus? Status { get; set; }
        public bool IsComplete { get; set; }
        public bool IsOfficiallyAssigned { get; set; }

        //public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }
        //public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }

        //public Instant RecordCreatedAt
        //{
        //    get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
        //    set => RecordCreatedAtAsDateTimeOffset = value.ToDateTimeOffset();
        //}

        //public Instant LastChangedOn
        //{
        //    get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
        //    set => LastChangedOnAsDateTimeOffset = value.ToDateTimeOffset();
        //}

        //public Application? Application { get; set; }
        //public Modification? Modification { get; set; }
        //public string? Operator { get; set; }
        //public Organisation? Organisation { get; set; }
        //public string? Reason { get; set; }
        //public string? EventDataAsXml { get; set; }
        //public DateTimeOffset SyndicationItemCreatedAt { get; set; }
    }

    public class AddressBoxNumberSyndicationConfiguration : IEntityTypeConfiguration<AddressBoxNumberSyndicationItem>
    {
        private const string TableName = "AddressBoxNumberSyndication";

        public void Configure(EntityTypeBuilder<AddressBoxNumberSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();
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
