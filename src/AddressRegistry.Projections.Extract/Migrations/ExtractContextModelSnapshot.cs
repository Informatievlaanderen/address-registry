﻿// <auto-generated />
using System;
using AddressRegistry.Projections.Extract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AddressRegistry.Projections.Extract.Migrations
{
    [DbContext(typeof(ExtractContext))]
    partial class ExtractContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AddressRegistry.Projections.Extract.AddressExtract.AddressExtractItemV2", b =>
                {
                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

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

                    b.Property<byte[]>("ShapeRecordContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.Property<int>("StreetNamePersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("AddressPersistentLocalId"));

                    b.HasIndex("StreetNamePersistentLocalId");

                    b.ToTable("AddressV2", "AddressRegistryExtract");
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

                    b.ToTable("ProjectionStates", "AddressRegistryExtract");
                });
#pragma warning restore 612, 618
        }
    }
}
