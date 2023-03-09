namespace AddressRegistry.Api.Oslo.AddressMatch.V1
{
    using System;
    using System.Collections.ObjectModel;
    using Microsoft.EntityFrameworkCore;

    public class BuildingContext : DbContext
    {
        public DbSet<BuildingUnitItem> BuildingUnits { get; set; }
        public DbSet<BuildingUnitAddress> BuildingUnitAddresses { get; set; }

        public BuildingContext()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public BuildingContext(DbContextOptions<BuildingContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var buildingSchema = "BuildingRegistryLegacy";

            modelBuilder
                .Entity<BuildingUnitItem>()
                .ToTable("BuildingUnitDetails", buildingSchema)
                .HasKey(p => p.BuildingUnitId);

            modelBuilder
                .Entity<BuildingUnitItem>()
                .HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => x.BuildingUnitId);

            modelBuilder
                .Entity<BuildingUnitAddress>()
                .ToTable("BuildingUnitAddresses", buildingSchema)
                .HasKey(p => new { p.BuildingUnitId, p.AddressId });
        }
    }

    public class BuildingUnitItem
    {
        public Guid BuildingUnitId { get; set; }
        public int? PersistentLocalId { get; set; }
        public Guid BuildingId { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsBuildingComplete { get; set; }

        public virtual Collection<BuildingUnitAddress> Addresses { get; set; }
    }

    public class BuildingUnitAddress
    {
        public Guid BuildingUnitId { get; set; }
        public Guid AddressId { get; set; }
    }
}
