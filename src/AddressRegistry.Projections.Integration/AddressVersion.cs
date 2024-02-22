namespace AddressRegistry.Projections.Integration
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using AddressRegistry.Infrastructure;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using StreetName;

    public sealed class AddressVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

        public long Position { get; set; }

        public int PersistentLocalId { get; set; }
        public Guid? AddressId { get; set; }
        public string? PostalCode { get; set; }
        public int? StreetNamePersistentLocalId { get; set; }
        public Guid? StreetNameId { get; set; }
        public AddressStatus? Status { get; set; }
        public string? OsloStatus { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public string Type { get; set; }
        public Geometry? Geometry { get; set; }
        public GeometryMethod? PositionMethod { get; set; }
        public string? OsloPositionMethod { get; set; }
        public GeometrySpecification? PositionSpecification { get; set; }
        public string? OsloPositionSpecification { get; set; }
        public bool? OfficiallyAssigned { get; set; }
        public bool Removed { get; set; }
        public string? PuriId { get; set; }
        public string? Namespace { get; set; }
        public string VersionAsString { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public string CreatedOnAsString { get; set; }
        private DateTimeOffset CreatedOnTimestampAsDateTimeOffset { get; set; }

        public Instant CreatedOnTimestamp
        {
            get => Instant.FromDateTimeOffset(CreatedOnTimestampAsDateTimeOffset);
            set
            {
                CreatedOnTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                CreatedOnAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }


        public AddressVersion()
        { }

        public AddressVersion CloneAndApplyEventInfo(long newPosition,
            string eventName,
            Instant lastChangedOn,
            Action<AddressVersion> editFunc)
        {
            var newItem = new AddressVersion
            {
                Position = newPosition,
                PersistentLocalId = PersistentLocalId,
                AddressId = AddressId,
                PostalCode = PostalCode,
                StreetNamePersistentLocalId = StreetNamePersistentLocalId,
                StreetNameId = StreetNameId,
                Status = Status,
                OsloStatus = OsloStatus,
                HouseNumber = HouseNumber,
                BoxNumber = BoxNumber,
                Type = eventName,
                Geometry = Geometry,
                PositionMethod = PositionMethod,
                PositionSpecification = PositionSpecification,
                OfficiallyAssigned = OfficiallyAssigned,
                Removed = Removed,
                PuriId = PuriId,
                Namespace = Namespace,
                VersionTimestamp = lastChangedOn,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public sealed class AddressVersionConfiguration : IEntityTypeConfiguration<AddressVersion>
    {
        public void Configure(EntityTypeBuilder<AddressVersion> builder)
        {
            const string tableName = "address_versions";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new { x.Position, x.PersistentLocalId});

            builder.Property(x => x.Position).ValueGeneratedNever();

            builder.Property(x => x.AddressId).HasColumnName("address_id");
            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.PersistentLocalId).HasColumnName("persistent_local_id");
            builder.Property(x => x.PostalCode).HasColumnName("postal_code");
            builder.Property(x => x.StreetNamePersistentLocalId).HasColumnName("street_name_persistent_local_id");
            builder.Property(x => x.StreetNameId).HasColumnName("street_name_id");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.OsloStatus).HasColumnName("oslo_status");
            builder.Property(x => x.HouseNumber).HasColumnName("house_number");
            builder.Property(x => x.BoxNumber).HasColumnName("box_number");
            builder.Property(x => x.Type).HasColumnName("type");

            builder.Property(x => x.Geometry).HasColumnName("geometry");

            builder.Property(x => x.PositionMethod).HasColumnName("position_method");
            builder.Property(x => x.OsloPositionMethod).HasColumnName("oslo_position_method");
            builder.Property(x => x.PositionSpecification).HasColumnName("position_specification");
            builder.Property(x => x.OsloPositionSpecification).HasColumnName("oslo_position_specification");
            builder.Property(x => x.OfficiallyAssigned).HasColumnName("officially_assigned");
            builder.Property(x => x.Removed).HasColumnName("removed");
            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");

            builder.Ignore(x => x.VersionTimestamp);
            builder.Property(AddressVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");

            builder.Ignore(x => x.CreatedOnTimestamp);
            builder.Property(AddressVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");


            builder.Property(x => x.PersistentLocalId).IsRequired();
            builder.HasIndex(x => x.PersistentLocalId);

            builder.HasIndex(x => x.Geometry).HasMethod("GIST");

            builder.HasIndex(x => x.StreetNamePersistentLocalId);
            builder.HasIndex(x => x.StreetNameId);
            builder.HasIndex(x => x.AddressId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.OsloStatus);
            builder.HasIndex(x => x.Type);
            builder.HasIndex(x => x.PostalCode);
            builder.HasIndex(x => x.HouseNumber);
            builder.HasIndex(x => x.BoxNumber);
            builder.HasIndex(x => x.Removed);
            builder.HasIndex(AddressVersion.VersionTimestampBackingPropertyName);
        }
    }
}
