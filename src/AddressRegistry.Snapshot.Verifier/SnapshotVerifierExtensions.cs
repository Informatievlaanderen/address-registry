namespace AddressRegistry.Snapshot.Verifier
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public static class SnapshotVerifierExtensions
    {
        public static IServiceCollection AddHostedSnapshotVerifierService<TAggregate, TAggregateId>(
            this IServiceCollection services,
            Func<TAggregate> aggregateFactory,
            Func<TAggregate, TAggregateId> aggregateIdFactory,
            List<string> membersToIgnoreInVerification)
            where TAggregate : class, IAggregateRootEntity, ISnapshotable
            where TAggregateId : class
        {
            services.AddHostedService(x => new SnapshotVerifier<TAggregate, TAggregateId>(
                x.GetRequiredService<IHostApplicationLifetime>(),
                x.GetRequiredService<MsSqlSnapshotStoreVerificationQueries>(),
                x.GetRequiredService<EventDeserializer>(),
                x.GetRequiredService<EventMapping>(),
                x.GetRequiredService<IReadonlyStreamStore>(),
                aggregateFactory,
                aggregateIdFactory,
                membersToIgnoreInVerification,
                x.GetRequiredService<IDbContextFactory<SnapshotVerifierContext>>(),
                x.GetRequiredService<ILoggerFactory>()));

            return services;
        }
    }
}
