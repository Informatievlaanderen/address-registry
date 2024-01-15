namespace AddressRegistry.Tests.Integration
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Integration;

    public abstract class IntegrationProjectionTest<TProjection>
        where TProjection : ConnectedProjection<IntegrationContext>
    {
        protected ConnectedProjectionTest<IntegrationContext, TProjection> Sut { get; }

        protected IntegrationProjectionTest()
        {
            Sut = new ConnectedProjectionTest<IntegrationContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual IntegrationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<IntegrationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new IntegrationContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
