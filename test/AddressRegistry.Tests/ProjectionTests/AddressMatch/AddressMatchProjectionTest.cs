namespace AddressRegistry.Tests.ProjectionTests.AddressMatch
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.AddressMatch;

    public abstract class AddressMatchProjectionTest<TProjection>
        where TProjection : ConnectedProjection<AddressMatchContext>, new()
    {
        protected ConnectedProjectionTest<AddressMatchContext, TProjection> Sut { get; }

        protected AddressMatchProjectionTest()
        {
            Sut = new ConnectedProjectionTest<AddressMatchContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual AddressMatchContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AddressMatchContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AddressMatchContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
