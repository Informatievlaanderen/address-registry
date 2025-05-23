﻿// <auto-generated />
using System;
using AddressRegistry.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AddressRegistry.Consumer.Migrations
{
    [DbContext(typeof(ConsumerContext))]
    partial class ConsumerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AddressRegistry.Consumer.StreetName.StreetNameConsumerItem", b =>
                {
                    b.Property<Guid>("StreetNameId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("StreetNameId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("StreetNameId"), false);

                    b.HasIndex("PersistentLocalId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("PersistentLocalId"));

                    b.ToTable("StreetNameConsumer", "AddressRegistryConsumer");
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

                    b.ToTable("ProjectionStates", "AddressRegistryConsumer");
                });
#pragma warning restore 612, 618
        }
    }
}
