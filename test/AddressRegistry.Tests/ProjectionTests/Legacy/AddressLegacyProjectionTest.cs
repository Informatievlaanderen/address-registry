namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;

    public abstract class AddressLegacyProjectionTest<TProjection>
        where TProjection : ConnectedProjection<LegacyContext>, new()
    {
        protected ConnectedProjectionTest<LegacyContext, TProjection> Sut { get; }

        protected AddressLegacyProjectionTest()
        {
            Sut = new ConnectedProjectionTest<LegacyContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual LegacyContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<LegacyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LegacyContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
