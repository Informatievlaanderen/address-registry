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

    public class MunicipalityConsumerContext : RunnerDbContext<MunicipalityConsumerContext>
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

            return municipality;
        }

        private static ProjectionItemNotFoundException<MunicipalityLatestItemProjections> DatabaseItemNotFound(Guid municipalityId)
            => new ProjectionItemNotFoundException<MunicipalityLatestItemProjections>(municipalityId.ToString("D"));
    }
}
