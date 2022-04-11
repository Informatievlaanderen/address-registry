namespace AddressRegistry.Tests.ProjectionTests.Consumer
{
    using System;
    using AddressRegistry.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;

    public class ConsumerProjectionTest<TProjection>
        where TProjection : ConnectedProjection<ConsumerContext>, new()
    {
        protected ConnectedProjectionTest<ConsumerContext, TProjection> Sut { get; }

        public ConsumerProjectionTest()
        {
            Sut = new ConnectedProjectionTest<ConsumerContext, TProjection>(CreateContext);
        }

        protected virtual ConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerContext(options);
        }
    }
}
