namespace AddressRegistry.Consumer.Read.Municipality
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Projections;
    using StreetName;
    using StreetName.DataStructures;

    public class MunicipalityConsumerContext : RunnerDbContext<MunicipalityConsumerContext>, IMunicipalities
    {
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<MunicipalityBosaItem> MunicipalityBosaItems { get; set; }

        // This needs to be here to please EF
        public MunicipalityConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public MunicipalityConsumerContext(DbContextOptions<MunicipalityConsumerContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerReadMunicipality;

        public MunicipalityData Get(MunicipalityId municipalityId)
        {
            var municipality = MunicipalityLatestItems.Find((Guid)municipalityId);

            return new MunicipalityData(municipalityId, municipality!.ExtendedWkbGeometry);
        }
    }

    //Classed used when running dotnet ef migrations
    public class ConsumerContextFactory : RunnerDbContextMigrationFactory<MunicipalityConsumerContext>
    {
        public ConsumerContextFactory()
            : this("ConsumerAdmin")
        { }

        public ConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.ConsumerReadMunicipality,
                Table = MigrationTables.ConsumerReadMunicipality
            })
        { }

        protected override MunicipalityConsumerContext CreateContext(DbContextOptions<MunicipalityConsumerContext> migrationContextOptions) => new MunicipalityConsumerContext(migrationContextOptions);

        public MunicipalityConsumerContext Create(DbContextOptions<MunicipalityConsumerContext> options) => CreateContext(options);
    }

    public static class AddressDetailExtensions
    {
        public static async Task<MunicipalityLatestItem> FindAndUpdate(
            this MunicipalityConsumerContext context,
            Guid municipalityId,
            Action<MunicipalityLatestItem> updateFunc,
            CancellationToken ct)
        {
            var municipality = await context
                .MunicipalityLatestItems
                .FindAsync(municipalityId, cancellationToken: ct);

            if (municipality == null)
                throw DatabaseItemNotFound(municipalityId);

            updateFunc(municipality);

            await context.SaveChangesAsync(ct);

            return municipality;
        }

        public static async Task<MunicipalityBosaItem> FindAndUpdate(
            this MunicipalityConsumerContext context,
            Guid municipalityId,
            Action<MunicipalityBosaItem> updateFunc,
            CancellationToken ct)
        {
            var municipality = await context
                .MunicipalityBosaItems
                .FindAsync(municipalityId, cancellationToken: ct);

            if (municipality == null)
                throw DatabaseItemNotFound(municipalityId);

            updateFunc(municipality);

            await context.SaveChangesAsync(ct);

            return municipality;
        }

        private static ProjectionItemNotFoundException<MunicipalityLatestItemProjections> DatabaseItemNotFound(Guid municipalityId)
            => new ProjectionItemNotFoundException<MunicipalityLatestItemProjections>(municipalityId.ToString("D"));
    }
}
