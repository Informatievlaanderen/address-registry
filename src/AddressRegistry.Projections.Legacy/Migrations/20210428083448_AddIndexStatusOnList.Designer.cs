﻿// <auto-generated />
using System;
using AddressRegistry.Projections.Legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    [Migration("20210428083448_AddIndexStatusOnList")]
    partial class AddIndexStatusOnList
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressDetail.AddressDetailItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Complete")
                        .HasColumnType("bit");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool?>("OfficiallyAssigned")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("Position")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("PositionMethod")
                        .HasColumnType("int");

                    b.Property<int?>("PositionSpecification")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.HasIndex("BoxNumber");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("PersistentLocalId")
                        .IsClustered();

                    b.HasIndex("PostalCode");

                    b.HasIndex("Status");

                    b.HasIndex("VersionTimestampAsDateTimeOffset");

                    b.HasIndex("Removed", "Complete");

                    b.HasIndex("StreetNameId", "Complete");

                    b.ToTable("AddressDetails", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressList.AddressListItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Complete")
                        .HasColumnType("bit");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.HasIndex("BoxNumber");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("PersistentLocalId")
                        .IsClustered();

                    b.HasIndex("PostalCode");

                    b.HasIndex("Status");

                    b.HasIndex("StreetNameId");

                    b.HasIndex("Complete", "Removed");

                    b.HasIndex("Complete", "Removed", "PersistentLocalId")
                        .IncludeProperties(new[] { "StreetNameId" });

                    b.ToTable("AddressList", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressList.AddressListViewCount", b =>
                {
                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.ToView("vw_AddressListCount", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressMatch.KadStreetName", b =>
                {
                    b.Property<string>("KadStreetNameCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StreetNameId")
                        .HasColumnType("int");

                    b.ToView("KadStreetNames", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressMatch.RRAddress", b =>
                {
                    b.Property<int>("AddressId")
                        .HasColumnType("int");

                    b.Property<string>("AddressType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RRHouseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RRIndex")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreetCode")
                        .HasColumnType("nvarchar(max)");

                    b.ToView("RRAddresses", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressMatch.RRStreetName", b =>
                {
                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreetCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreetName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StreetNameId")
                        .HasColumnType("int");

                    b.ToView("RRStreetNames", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressSyndication.AddressSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("AddressId")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("Application")
                        .HasColumnType("int");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ChangeType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventDataAsXml")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsOfficiallyAssigned")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("LastChangedOn");

                    b.Property<int?>("Modification")
                        .HasColumnType("int");

                    b.Property<string>("Operator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Organisation")
                        .HasColumnType("int");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("PointPosition")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("PositionMethod")
                        .HasColumnType("int");

                    b.Property<int?>("PositionSpecification")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("RecordCreatedAtAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("RecordCreatedAt");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<Guid?>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("SyndicationItemCreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Position")
                        .IsClustered();

                    b.HasIndex("AddressId");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("Position")
                        .HasDatabaseName("CI_AddressSyndication_Position")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("AddressSyndication", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId.CrabIdToPersistentLocalIdItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("HouseNumberId")
                        .HasColumnType("int");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("SubaddressId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.HasIndex("HouseNumberId")
                        .IsClustered();

                    b.HasIndex("IsRemoved");

                    b.HasIndex("PersistentLocalId")
                        .IsUnique()
                        .HasFilter("[PersistentLocalId] IS NOT NULL");

                    b.HasIndex("SubaddressId");

                    b.ToTable("CrabIdToPersistentLocalIds", "AddressRegistryLegacy");
                });

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .IsClustered();

                    b.ToTable("ProjectionStates", "AddressRegistryLegacy");
                });
#pragma warning restore 612, 618
        }
    }
}