namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Xunit.Abstractions;

    public abstract class ProjectionTest<TContext, TProjection>
        where TProjection : ConnectedProjection<TContext>
        where TContext : DbContext
    {
        private readonly XUnitLogger _logger;

        protected ProjectionTest(ITestOutputHelper testOutputHelper)
        {
            _logger = new XUnitLogger(testOutputHelper);
        }

        protected async Task Assert(ConnectedProjectionTestSpecification<TContext> specification)
        {
            await specification.Assert(CreateContext, _logger);
        }

        protected ConnectedProjectionScenario<TContext> Given(params object[] messages)
        {
            var projection = CreateProjection();
            var resolver = ConcurrentResolve.WhenEqualToHandlerMessageType(projection.Handlers);

            return new ConnectedProjectionScenario<TContext>(resolver)
                .Given(messages);
        }

        protected abstract TContext CreateContext(DbContextOptions<TContext> options);
        protected abstract TProjection CreateProjection();

        private TContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = CreateContext(options);
            context.Database.EnsureDeleted();
            return context;
        }
    }
}
