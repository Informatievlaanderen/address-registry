namespace AddressRegistry.Tests.ProjectionTests.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using StreetName = AddressRegistry.Consumer.Read.StreetName;

    public abstract class StreetNameProjectionTest<TProjection>
        where TProjection : ConnectedProjection<StreetName.StreetNameConsumerContext>, new()
    {
        protected ConnectedProjectionTest<StreetName.StreetNameConsumerContext, TProjection> Sut { get; }

        public StreetNameProjectionTest()
        {
            Sut = new ConnectedProjectionTest<StreetName.StreetNameConsumerContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual StreetName.StreetNameConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<StreetName.StreetNameConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new StreetName.StreetNameConsumerContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
