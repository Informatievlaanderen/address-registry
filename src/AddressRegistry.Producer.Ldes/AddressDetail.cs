namespace AddressRegistry.Producer.Ldes
{
    using System;
    using AddressRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using StreetName;

    public class AddressDetail
    {
        public int AddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public int? ParentAddressPersistentLocalId { get; set; }
        public string NisCode { get; set; }
        public string? PostalCode { get; set; }
        public string HouseNumber { get; set; } = string.Empty;
        public string? BoxNumber { get; set; }
        public AddressStatus Status { get; set; }
        public bool OfficiallyAssigned { get; set; }

        public byte[] Position { get; set; }
        public GeometryMethod PositionMethod { get; set; }
        public GeometrySpecification PositionSpecification { get; set; }

        public bool Removed { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }
        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        protected AddressDetail()
        { }

        public AddressDetail(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            int? parentAddressPersistentLocalId,
            string nisCode,
            string? postalCode,
            string houseNumber,
            string? boxNumber,
            AddressStatus status,
            bool officiallyAssigned,
            byte[] position,
            GeometryMethod positionMethod,
            GeometrySpecification positionSpecification,
            bool removed,
            Instant versionTimestamp)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            ParentAddressPersistentLocalId = parentAddressPersistentLocalId;
            NisCode = nisCode;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            Status = status;
            OfficiallyAssigned = officiallyAssigned;
            Position = position;
            PositionMethod = positionMethod;
            PositionSpecification = positionSpecification;
            Removed = removed;
            VersionTimestamp = versionTimestamp;
        }
    }

    public class AddressDetailConfiguration : IEntityTypeConfiguration<AddressDetail>
    {
        internal const string TableName = "Address";

        public void Configure(EntityTypeBuilder<AddressDetail> b)
        {
            b.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => p.AddressPersistentLocalId)
                .IsClustered();

            b.Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName(nameof(AddressDetail.VersionTimestamp));
            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNamePersistentLocalId);
            b.Property(p => p.ParentAddressPersistentLocalId);
            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.OfficiallyAssigned);
            b.Property(p => p.Position);
            b.Property(p => p.PositionSpecification);
            b.Property(p => p.PositionMethod);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);

            b.HasIndex(p => p.StreetNamePersistentLocalId);
            b.HasIndex(x => x.NisCode);
        }
    }
}
