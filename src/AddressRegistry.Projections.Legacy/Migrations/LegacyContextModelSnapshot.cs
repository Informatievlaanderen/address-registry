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
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressDetail.AddressDetailItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BoxNumber");

                    b.Property<bool>("Complete");

                    b.Property<string>("HouseNumber");

                    b.Property<bool?>("OfficiallyAssigned");

                    b.Property<int?>("OsloId");

                    b.Property<byte[]>("Position");

                    b.Property<int?>("PositionMethod");

                    b.Property<int?>("PositionSpecification");

                    b.Property<string>("PostalCode");

                    b.Property<bool>("Removed");

                    b.Property<int?>("Status");

                    b.Property<Guid>("StreetNameId");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("OsloId");

                    b.ToTable("AddressDetails","AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressList.AddressListItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BoxNumber");

                    b.Property<bool>("Complete");

                    b.Property<string>("HouseNumber");

                    b.Property<int>("OsloId");

                    b.Property<string>("PostalCode");

                    b.Property<bool>("Removed");

                    b.Property<Guid>("StreetNameId");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("BoxNumber");

                    b.HasIndex("HouseNumber");

                    b.HasIndex("PostalCode");

                    b.HasIndex("Complete", "Removed");

                    b.ToTable("AddressList","AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressSyndication.AddressSyndicationItem", b =>
                {
                    b.Property<long>("Position");

                    b.Property<Guid?>("AddressId")
                        .IsRequired();

                    b.Property<int?>("Application");

                    b.Property<string>("BoxNumber");

                    b.Property<string>("ChangeType");

                    b.Property<string>("HouseNumber");

                    b.Property<bool>("IsComplete");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnName("LastChangedOn");

                    b.Property<int?>("Modification");

                    b.Property<string>("Operator");

                    b.Property<int?>("Organisation");

                    b.Property<int?>("OsloId");

                    b.Property<int?>("Plan");

                    b.Property<string>("PostalCode");

                    b.Property<DateTimeOffset>("RecordCreatedAtAsDateTimeOffset")
                        .HasColumnName("RecordCreatedAt");

                    b.Property<int?>("Status");

                    b.Property<Guid?>("StreetNameId");

                    b.HasKey("Position")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("AddressId");

                    b.HasIndex("OsloId");

                    b.ToTable("AddressSyndication","AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.AddressVersion.AddressVersion", b =>
                {
                    b.Property<Guid>("AddressId");

                    b.Property<long>("StreamPosition");

                    b.Property<int?>("Application");

                    b.Property<string>("BoxNumber");

                    b.Property<bool>("Complete");

                    b.Property<string>("HouseNumber");

                    b.Property<int?>("Modification");

                    b.Property<bool?>("OfficiallyAssigned");

                    b.Property<string>("Operator");

                    b.Property<int?>("Organisation");

                    b.Property<int>("OsloId");

                    b.Property<int?>("Plan");

                    b.Property<byte[]>("Position");

                    b.Property<int?>("PositionMethod");

                    b.Property<int?>("PositionSpecification");

                    b.Property<string>("PostalCode");

                    b.Property<bool>("Removed");

                    b.Property<int?>("Status");

                    b.Property<Guid>("StreetNameId");

                    b.Property<DateTimeOffset?>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId", "StreamPosition")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("OsloId");

                    b.ToTable("AddressVersions","AddressRegistryLegacy");
                });

            modelBuilder.Entity("AddressRegistry.Projections.Legacy.CrabIdToOsloId.CrabIdToOsloIdItem", b =>
                {
                    b.Property<Guid>("AddressId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BoxNumber");

                    b.Property<string>("HouseNumber");

                    b.Property<int?>("HouseNumberId");

                    b.Property<bool>("IsComplete");

                    b.Property<bool>("IsRemoved");

                    b.Property<int?>("OsloId");

                    b.Property<string>("PostalCode");

                    b.Property<Guid>("StreetNameId");

                    b.Property<int?>("SubaddressId");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("HouseNumberId");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("OsloId")
                        .IsUnique()
                        .HasFilter("[OsloId] IS NOT NULL");

                    b.HasIndex("SubaddressId");

                    b.ToTable("CrabIdToOsloIds","AddressRegistryLegacy");
                });

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Position");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","AddressRegistryLegacy");
                });
#pragma warning restore 612, 618
        }
    }
}
