namespace AddressRegistry.Projections.Legacy.AddressVersion
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class AddressVersion
    {
        public Guid AddressId { get; set; }
        public long StreamPosition { get; set; }

        public int PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public AddressStatus? Status { get; set; }
        public bool? OfficiallyAssigned { get; set; }

        public byte[] Position { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        public Instant? VersionTimestamp
        {
            get => VersionTimestampAsDateTimeOffset == null ? (Instant?)null : Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset.Value);
            set => VersionTimestampAsDateTimeOffset = value?.ToDateTimeOffset();
        }

        public DateTimeOffset? VersionTimestampAsDateTimeOffset { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }

        public AddressVersion CloneAndApplyEventInfo(
            long newStreamPosition,
            Action<AddressVersion> editFunc)
        {
            var newItem = new AddressVersion
            {
                AddressId = AddressId,
                StreamPosition = newStreamPosition,

                PersistentLocalId = PersistentLocalId,

                Status = Status,
                OfficiallyAssigned = OfficiallyAssigned,

                StreetNameId = StreetNameId,
                BoxNumber = BoxNumber,
                HouseNumber = HouseNumber,
                PostalCode = PostalCode,

                Position = Position,
                PositionSpecification = PositionSpecification,
                PositionMethod = PositionMethod,

                Complete = Complete,
                Removed = Removed,

                Reason = Reason,
                Modification = Modification,
                VersionTimestamp = VersionTimestamp,
                Application = Application,
                Operator = Operator,
                Organisation = Organisation,
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public class AddressHistItemConfiguration : IEntityTypeConfiguration<AddressVersion>
    {
        private const string TableName = "AddressVersions";

        public void Configure(EntityTypeBuilder<AddressVersion> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.AddressId, p.StreamPosition })
                .IsClustered(false);

            b.HasIndex(p => p.PersistentLocalId);

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

            b.Property(x => x.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.Ignore(x => x.VersionTimestamp);
        }
    }
}
