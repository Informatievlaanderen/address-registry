﻿// <auto-generated />
using System;
using AddressRegistry.Projections.Legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    partial class LegacyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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
                        .HasColumnName("VersionTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("BoxNumber");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("PostalCode");

                    b.HasIndex("Status");

                    b.HasIndex("VersionTimestampAsDateTimeOffset");

                    b.HasIndex("Removed", "Complete");

                    b.HasIndex("StreetNameId", "Complete");

                    b.ToTable("AddressDetails","AddressRegistryLegacy");
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

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("BoxNumber");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("PostalCode");

                    b.HasIndex("StreetNameId");

                    b.HasIndex("Complete", "Removed");

                    b.HasIndex("Complete", "Removed", "PersistentLocalId");

                    b.ToTable("AddressList","AddressRegistryLegacy");
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
                        .HasColumnName("LastChangedOn")
                        .HasColumnType("datetimeoffset");

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
                        .HasColumnName("RecordCreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<Guid?>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Position")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("AddressId");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("Position")
                        .HasName("CI_AddressSyndication_Position")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("AddressSyndication","AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressVersion.AddressVersion", b =>
                {
                    b.Property<Guid>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("StreamPosition")
                        .HasColumnType("bigint");

                    b.Property<int?>("Application")
                        .HasColumnType("int");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Complete")
                        .HasColumnType("bit");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Modification")
                        .HasColumnType("int");

                    b.Property<bool?>("OfficiallyAssigned")
                        .HasColumnType("bit");

                    b.Property<string>("Operator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Organisation")
                        .HasColumnType("int");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("Position")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("PositionMethod")
                        .HasColumnType("int");

                    b.Property<int?>("PositionSpecification")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("AddressId", "StreamPosition")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("AddressVersions","AddressRegistryLegacy");
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
                        .HasColumnName("VersionTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("HouseNumberId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("IsRemoved");

                    b.HasIndex("PersistentLocalId")
                        .IsUnique()
                        .HasFilter("[PersistentLocalId] IS NOT NULL");

                    b.HasIndex("SubaddressId");

                    b.ToTable("CrabIdToPersistentLocalIds","AddressRegistryLegacy");
                });

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","AddressRegistryLegacy");
                });
#pragma warning restore 612, 618
        }
    }
}
