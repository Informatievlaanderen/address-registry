namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using StreetName;

    public class AddressSyndicationItem
    {
        public long FeedPosition { get; set; }

        [Obsolete("Guid identifiers are no longer used.")]
        public Guid? AddressId { get; set; }
        [Obsolete("Guid identifiers are no longer used.")]
        public Guid? StreetNameId { get; set; }

        public int? PersistentLocalId { get; set; }
        public int? StreetNamePersistentLocalId { get; set; }
        public long Position { get; set; }
        public string? ChangeType { get; set; }
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
                StreetNamePersistentLocalId = StreetNamePersistentLocalId,
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

        public AddressSyndicationItem CloneAndApplyEventInfoForBoxNumber(
            AddressBoxNumberSyndicationHelper addressBoxNumberSyndicationHelper,
            long position,
            string eventName,
            Instant timestamp,
            Action<AddressSyndicationItem> applyEventInfoOn)
        {
            var newApply = new Action<AddressSyndicationItem>((item) =>
            {
                item.PostalCode = addressBoxNumberSyndicationHelper.PostalCode;
                item.HouseNumber = addressBoxNumberSyndicationHelper.HouseNumber;
                item.BoxNumber = addressBoxNumberSyndicationHelper.BoxNumber;
                item.PointPosition = addressBoxNumberSyndicationHelper.PointPosition;
                item.PositionMethod = addressBoxNumberSyndicationHelper.PositionMethod;
                item.PositionSpecification = addressBoxNumberSyndicationHelper.PositionSpecification;
                item.Status = addressBoxNumberSyndicationHelper.Status;
                item.IsComplete = addressBoxNumberSyndicationHelper.IsComplete;
                item.IsOfficiallyAssigned = addressBoxNumberSyndicationHelper.IsOfficiallyAssigned;

                applyEventInfoOn(item);
            });

            return CloneAndApplyEventInfo(
                position,
                eventName,
                timestamp,
                newApply);
        }
    }

    public class AddressSyndicationConfiguration : IEntityTypeConfiguration<AddressSyndicationItem>
    {
        private const string TableName = "AddressSyndication";

        public void Configure(EntityTypeBuilder<AddressSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.FeedPosition)
                .IsClustered();

            b.Property(x => x.FeedPosition).UseIdentityColumn();
            b.HasIndex(x => x.FeedPosition).IsColumnStore($"CI_{TableName}_FeedPosition");

            b.Property(x => x.Position).ValueGeneratedNever();
            b.HasIndex(x => x.Position);

            b.Property(x => x.AddressId);
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
