﻿// <auto-generated />
using AddressRegistry.Snapshot.Verifier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AddressRegistry.Snapshot.Verifier.Migrations
{
    [DbContext(typeof(SnapshotVerifierContext))]
    partial class SnapshotVerifierContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("AddressRegistry.Snapshot.Verifier.SnapshotVerificationState", b =>
                {
                    b.Property<int>("SnapshotId")
                        .HasColumnType("int");

                    b.Property<string>("Differences")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("SnapshotId");

                    b.HasIndex("Status");

                    b.ToTable("SnapshotVerificationStates", "AddressRegistry");
                });
#pragma warning restore 612, 618
        }
    }
}
