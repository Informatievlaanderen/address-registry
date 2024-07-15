namespace AddressRegistry.Tests.ProjectionTests.Postal
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Postal = AddressRegistry.Consumer.Read.Postal;

    public abstract class MunicipalityProjectionTest<TProjection>
        where TProjection : ConnectedProjection<Postal.PostalConsumerContext>, new()
    {
        protected ConnectedProjectionTest<Postal.PostalConsumerContext, TProjection> Sut { get; }

        public MunicipalityProjectionTest()
        {
            Sut = new ConnectedProjectionTest<Postal.PostalConsumerContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual Postal.PostalConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<Postal.PostalConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new Postal.PostalConsumerContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
