﻿namespace AddressRegistry.Projections.Integration
{
    using System;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public sealed class AddressLatestItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public string? NisCode { get; set; }
        public string? PostalCode { get; set; }
        public int? StreetNamePersistentLocalId { get; set; }
        public string? Status { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public Geometry? Geometry { get; set; }
        public string? PositionMethod { get; set; }
        public string? PositionSpecification { get; set; }
        public bool? OfficiallyAssigned { get; set; }
        public bool Removed { get; set; }
        public string? PuriId { get; set; }
        public string? Namespace { get; set; }
        public string VersionAsString { get; set; }
        public long IdempotenceKey { get; set; }

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

        public AddressLatestItem()
        { }
    }

    public sealed class AddressLatestItemConfiguration : IEntityTypeConfiguration<AddressLatestItem>
    {
        public void Configure(EntityTypeBuilder<AddressLatestItem> builder)
        {
            const string geomFromGmlComputedQuery = "ST_GeomFromGML(REPLACE(\"GeometryGml\",'https://www.opengis.net/def/crs/EPSG/0/', 'EPSG:')) ";

            const string tableName = "municipality_versions";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => x.PersistentLocalId);

            builder.Property(x => x.PersistentLocalId).ValueGeneratedNever();

            builder.Property(x => x.PersistentLocalId).HasColumnName("persistent_local_id");
            builder.Property(x => x.NisCode).HasColumnName("nis_code")
                .HasMaxLength(5)
                .IsFixedLength();;
            builder.Property(x => x.PostalCode).HasColumnName("postal_code");
            builder.Property(x => x.StreetNamePersistentLocalId).HasColumnName("street_name_persistent_local_id");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.HouseNumber).HasColumnName("house_number");
            builder.Property(x => x.BoxNumber).HasColumnName("box_number");

            builder.Property(x => x.Geometry).HasColumnName("geometry");
            builder.Property(x => x.Geometry)
                .HasComputedColumnSql(geomFromGmlComputedQuery, stored: true);

            builder.Property(x => x.PositionMethod).HasColumnName("position_method");
            builder.Property(x => x.PositionSpecification).HasColumnName("position_specification");
            builder.Property(x => x.OfficiallyAssigned).HasColumnName("officially_assigned");
            builder.Property(x => x.Removed).HasColumnName("removed");
            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(AddressVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");

            builder.Ignore(x => x.VersionTimestamp);

            builder.Property(x => x.IdempotenceKey).HasColumnName("idempotence_key");

            builder.Property(x => x.PersistentLocalId).IsRequired();
            builder.HasIndex(x => x.PersistentLocalId);

            builder.HasIndex(x => x.Geometry).HasMethod("GIST");

            builder.HasIndex(x => x.StreetNamePersistentLocalId);
            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PostalCode);
            builder.HasIndex(x => x.HouseNumber);
            builder.HasIndex(x => x.BoxNumber);
            builder.HasIndex(x => x.Removed);
        }
    }
}
