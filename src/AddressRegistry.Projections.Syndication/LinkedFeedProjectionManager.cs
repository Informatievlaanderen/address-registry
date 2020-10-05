namespace AddressRegistry.Projections.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class LinkedFeedProjectionManager<TContext>: IFeedProjectionRunner<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly IEnumerable<ILinkedFeedProjectionRunner<TContext>> _linkedFeedProjectionRunners;

        public LinkedFeedProjectionManager(IEnumerable<ILinkedFeedProjectionRunner<TContext>> linkedFeedProjectionRunners)
        {
            _linkedFeedProjectionRunners = linkedFeedProjectionRunners;
        }

        public async Task CatchUpAsync(Func<Owned<TContext>> context, CancellationToken cancellationToken)
        {
            while (true)
            {
                foreach (var linkedFeedProjectionRunner in _linkedFeedProjectionRunners)
                    await linkedFeedProjectionRunner.CatchUpAsync(context, cancellationToken);

                Thread.Sleep(10000 / _linkedFeedProjectionRunners.Count());
            }
        }
    }
}
