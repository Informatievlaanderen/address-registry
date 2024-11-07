﻿// <auto-generated />
using AddressRegistry.Api.BackOffice.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AddressRegistry.Api.BackOffice.Abstractions.Migrations
{
    [DbContext(typeof(BackOfficeContext))]
    partial class BackOfficeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AddressRegistry.Api.BackOffice.Abstractions.AddressPersistentIdStreetNamePersistentId", b =>
                {
                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("StreetNamePersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("AddressPersistentLocalId"));

                    b.ToTable("AddressPersistentIdStreetNamePersistentId", "AddressRegistryBackOffice");
                });

            modelBuilder.Entity("AddressRegistry.Api.BackOffice.Abstractions.MunicipalityMergerAddress", b =>
                {
                    b.Property<int>("OldAddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("NewAddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("NewStreetNamePersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("OldStreetNamePersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("OldAddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("OldAddressPersistentLocalId"));

                    b.HasIndex("OldStreetNamePersistentLocalId");

                    b.ToTable("MunicipalityMergerAddresses", "AddressRegistryBackOffice");
                });
#pragma warning restore 612, 618
        }
    }
}
