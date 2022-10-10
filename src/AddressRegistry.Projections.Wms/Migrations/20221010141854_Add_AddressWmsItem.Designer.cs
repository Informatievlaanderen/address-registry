﻿// <auto-generated />
using System;
using AddressRegistry.Projections.Wms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    [DbContext(typeof(WmsContext))]
    [Migration("20221010141854_Add_AddressWmsItem")]
    partial class Add_AddressWmsItem
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("AddressRegistry.Projections.Wms.AddressDetail.AddressDetailItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Complete")
                        .HasColumnType("bit");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HouseNumberLabel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("HouseNumberLabelLength")
                        .HasColumnType("int");

                    b.Property<int>("LabelType")
                        .HasColumnType("int");

                    b.Property<bool?>("OfficiallyAssigned")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<Point>("Position")
                        .HasColumnType("sys.geometry");

                    b.Property<string>("PositionMethod")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PositionSpecification")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("PositionX")
                        .HasColumnType("float");

                    b.Property<double?>("PositionY")
                        .HasColumnType("float");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("VersionAsString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("AddressId"));

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("Status");

                    b.HasIndex("StreetNameId");

                    b.HasIndex("PositionX", "PositionY", "Removed", "Complete", "Status");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("PositionX", "PositionY", "Removed", "Complete", "Status"), new[] { "StreetNameId" });

                    b.ToTable("AddressDetails", "wms.address");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Wms.AddressWmsItem.AddressWmsItem", b =>
                {
                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HouseNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HouseNumberLabel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("HouseNumberLabelLength")
                        .HasColumnType("int");

                    b.Property<int>("LabelType")
                        .HasColumnType("int");

                    b.Property<bool>("OfficiallyAssigned")
                        .HasColumnType("bit");

                    b.Property<Point>("Position")
                        .IsRequired()
                        .HasColumnType("sys.geometry");

                    b.Property<string>("PositionMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PositionSpecification")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("PositionX")
                        .HasColumnType("float");

                    b.Property<double>("PositionY")
                        .HasColumnType("float");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("StreetNamePersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("VersionAsString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("AddressPersistentLocalId"));

                    b.HasIndex("Status");

                    b.HasIndex("StreetNamePersistentLocalId");

                    b.HasIndex("PositionX", "PositionY", "Removed", "Status");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("PositionX", "PositionY", "Removed", "Status"), new[] { "StreetNamePersistentLocalId" });

                    b.ToTable("AddressWms", "wms.address");
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

                    b.HasKey("Name");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Name"));

                    b.ToTable("ProjectionStates", "wms.address");
                });
#pragma warning restore 612, 618
        }
    }
}
