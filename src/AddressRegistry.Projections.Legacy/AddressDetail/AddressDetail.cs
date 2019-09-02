namespace AddressRegistry.Projections.Legacy.AddressDetail
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class AddressDetailItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid AddressId { get; set; }
        public int? PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string PostalCode { get; set; }
        public string HouseNumber { get; set; }
        public string BoxNumber { get; set; }
        public AddressStatus? Status { get; set; }
        public bool? OfficiallyAssigned { get; set; }

        public byte[] Position { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class AddressDetailItemConfiguration : IEntityTypeConfiguration<AddressDetailItem>
    {
        private const string TableName = "AddressDetails";

        public void Configure(EntityTypeBuilder<AddressDetailItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressId)
                .ForSqlServerIsClustered(false);

            b.HasIndex(p => p.PersistentLocalId);

            b.Property(AddressDetailItem.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNameId);
            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.OfficiallyAssigned);
            b.Property(p => p.Position);
            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Complete);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);
        }
    }
}
