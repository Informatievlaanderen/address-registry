namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class AddressSyndicationItem
    {
        public long Position { get; set; }

        public Guid? AddressId { get; set; }
        public int? PersistentLocalId { get; set; }
        public string? ChangeType { get; set; }

        public Guid? StreetNameId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public byte[]? PointPosition { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }

        public AddressStatus? Status { get; set; }
        public bool IsComplete { get; set; }
        public bool IsOfficiallyAssigned { get; set; }

        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }
        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set => LastChangedOnAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string? EventDataAsXml { get; set; }
        public DateTimeOffset SyndicationItemCreatedAt { get; set; }

        public AddressSyndicationItem CloneAndApplyEventInfo(
            long position,
            string changeType,
            Instant lastChangedOn,
            Action<AddressSyndicationItem> editFunc)
        {
            var newItem = new AddressSyndicationItem
            {
                ChangeType = changeType,
                Position = position,
                LastChangedOn = lastChangedOn,

                AddressId = AddressId,
                PersistentLocalId = PersistentLocalId,
                StreetNameId = StreetNameId,
                PostalCode = PostalCode,
                HouseNumber = HouseNumber,
                BoxNumber = BoxNumber,
                Status = Status,
                PointPosition = PointPosition,
                PositionMethod = PositionMethod,
                PositionSpecification = PositionSpecification,
                IsComplete = IsComplete,
                IsOfficiallyAssigned = IsOfficiallyAssigned,
                RecordCreatedAt = RecordCreatedAt,
                Application = Application,
                Modification = Modification,
                Operator = Operator,
                Organisation = Organisation,
                Reason = Reason,
                SyndicationItemCreatedAt = DateTimeOffset.UtcNow
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public class AddressSyndicationConfiguration : IEntityTypeConfiguration<AddressSyndicationItem>
    {
        private const string TableName = "AddressSyndication";

        public void Configure(EntityTypeBuilder<AddressSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.Position)
                .IsClustered();

            b.Property(x => x.Position).ValueGeneratedNever();
            b.HasIndex(x => x.Position).IsColumnStore($"CI_{TableName}_Position");

            b.Property(x => x.AddressId).IsRequired();
            b.Property(x => x.ChangeType);

            b.Property(x => x.StreetNameId);
            b.Property(x => x.PostalCode);
            b.Property(x => x.HouseNumber);
            b.Property(x => x.BoxNumber);

            b.Property(x => x.Status);
            b.Property(x => x.IsOfficiallyAssigned);
            b.Property(x => x.IsComplete);

            b.Property(x => x.PointPosition);
            b.Property(x => x.PositionMethod);
            b.Property(x => x.PositionSpecification);

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset).HasColumnName("RecordCreatedAt");
            b.Property(x => x.LastChangedOnAsDateTimeOffset).HasColumnName("LastChangedOn");

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);
            b.Property(x => x.EventDataAsXml);
            b.Property(x => x.SyndicationItemCreatedAt).IsRequired();

            b.Ignore(x => x.RecordCreatedAt);
            b.Ignore(x => x.LastChangedOn);

            b.HasIndex(x => x.AddressId);
            b.HasIndex(x => x.PersistentLocalId);
        }
    }
}
