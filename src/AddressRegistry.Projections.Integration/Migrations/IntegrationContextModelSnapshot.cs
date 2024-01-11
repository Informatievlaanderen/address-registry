﻿// <auto-generated />
using System;
using AddressRegistry.Projections.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    [DbContext(typeof(IntegrationContext))]
    partial class IntegrationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AddressRegistry.Projections.Integration.AddressLatestItem", b =>
                {
                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("persistent_local_id");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("text")
                        .HasColumnName("box_number");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("text")
                        .HasColumnName("house_number");

                    b.Property<string>("Namespace")
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<bool?>("OfficiallyAssigned")
                        .HasColumnType("boolean")
                        .HasColumnName("officially_assigned");

                    b.Property<string>("OsloPositionMethod")
                        .HasColumnType("text")
                        .HasColumnName("oslo_position_method");

                    b.Property<string>("OsloPositionSpecification")
                        .HasColumnType("text")
                        .HasColumnName("oslo_position_specification");

                    b.Property<string>("OsloStatus")
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<int>("PositionMethod")
                        .HasColumnType("integer")
                        .HasColumnName("position_method");

                    b.Property<int>("PositionSpecification")
                        .HasColumnType("integer")
                        .HasColumnName("position_specification");

                    b.Property<string>("PostalCode")
                        .HasColumnType("text")
                        .HasColumnName("postal_code");

                    b.Property<string>("PuriId")
                        .HasColumnType("text")
                        .HasColumnName("puri_id");

                    b.Property<bool>("Removed")
                        .HasColumnType("boolean")
                        .HasColumnName("removed");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<int?>("StreetNamePersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("street_name_persistent_local_id");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("PersistentLocalId");

                    b.HasIndex("BoxNumber");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("PostalCode");

                    b.HasIndex("Removed");

                    b.HasIndex("Status");

                    b.HasIndex("StreetNamePersistentLocalId");

                    b.ToTable("address_latest_items", "integration_address");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Integration.AddressVersion", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint")
                        .HasColumnName("position");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("persistent_local_id");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uuid")
                        .HasColumnName("address_id");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("text")
                        .HasColumnName("box_number");

                    b.Property<string>("CreatedOnAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_on_as_string");

                    b.Property<DateTimeOffset>("CreatedOnTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_timestamp");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("text")
                        .HasColumnName("house_number");

                    b.Property<string>("Namespace")
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<bool?>("OfficiallyAssigned")
                        .HasColumnType("boolean")
                        .HasColumnName("officially_assigned");

                    b.Property<string>("OsloPositionMethod")
                        .HasColumnType("text")
                        .HasColumnName("oslo_position_method");

                    b.Property<string>("OsloPositionSpecification")
                        .HasColumnType("text")
                        .HasColumnName("oslo_position_specification");

                    b.Property<string>("OsloStatus")
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<int?>("PositionMethod")
                        .HasColumnType("integer")
                        .HasColumnName("position_method");

                    b.Property<int?>("PositionSpecification")
                        .HasColumnType("integer")
                        .HasColumnName("position_specification");

                    b.Property<string>("PostalCode")
                        .HasColumnType("text")
                        .HasColumnName("postal_code");

                    b.Property<string>("PuriId")
                        .HasColumnType("text")
                        .HasColumnName("puri_id");

                    b.Property<bool>("Removed")
                        .HasColumnType("boolean")
                        .HasColumnName("removed");

                    b.Property<int?>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<Guid?>("StreetNameId")
                        .HasColumnType("uuid")
                        .HasColumnName("street_name_id");

                    b.Property<int?>("StreetNamePersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("street_name_persistent_local_id");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("Position", "PersistentLocalId");

                    b.HasIndex("AddressId");

                    b.HasIndex("BoxNumber");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("PostalCode");

                    b.HasIndex("Removed");

                    b.HasIndex("Status");

                    b.HasIndex("StreetNameId");

                    b.HasIndex("StreetNamePersistentLocalId");

                    b.HasIndex("VersionTimestampAsDateTimeOffset");

                    b.ToTable("address_versions", "integration_address");
                });

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("DesiredState")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name");

                    b.ToTable("ProjectionStates", "integration_address");
                });
#pragma warning restore 612, 618
        }
    }
}
