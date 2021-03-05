﻿// <auto-generated />
using System;
using AddressRegistry.Projections.Extract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AddressRegistry.Projections.Extract.Migrations
{
    [DbContext(typeof(ExtractContext))]
    [Migration("20210305160746_SetDatabaseIsolationLevel")]
    partial class SetDatabaseIsolationLevel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract.AddressCrabHouseNumberIdExtractItem", b =>
                {
                    b.Property<Guid?>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("CrabHouseNumberId")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.ToTable("AddressIdCrabHouseNumberId", "AddressRegistryExtract");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract.AddressCrabSubaddressIdExtractItem", b =>
                {
                    b.Property<Guid?>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("CrabSubaddressId")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.ToTable("AddressIdCrabSubaddressId", "AddressRegistryExtract");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Extract.AddressExtract.AddressExtractItem", b =>
                {
                    b.Property<Guid?>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<bool>("Complete")
                        .HasColumnType("bit");

                    b.Property<byte[]>("DbaseRecord")
                        .HasColumnType("varbinary(max)");

                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.Property<string>("NisCode")
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("ShapeRecordContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.HasIndex("AddressPersistentLocalId")
                        .IsClustered();

                    b.HasIndex("Complete");

                    b.HasIndex("NisCode");

                    b.HasIndex("StreetNameId");

                    b.ToTable("Address", "AddressRegistryExtract");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Extract.AddressLinkExtract.AddressLinkExtractItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BoxNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Complete")
                        .HasColumnType("bit");

                    b.Property<byte[]>("DbaseRecord")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("HouseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StreetNameId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("AddressId")
                        .IsClustered(false);

                    b.HasIndex("PersistentLocalId")
                        .IsClustered();

                    b.ToTable("AddressLinks", "AddressRegistryExtract");
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

                    b.ToTable("ProjectionStates", "AddressRegistryExtract");
                });
#pragma warning restore 612, 618
        }
    }
}
