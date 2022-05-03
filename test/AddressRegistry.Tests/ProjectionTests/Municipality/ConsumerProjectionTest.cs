namespace AddressRegistry.Tests.ProjectionTests.Municipality
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Municipality = AddressRegistry.Consumer.Read.Municipality;

    public abstract class MunicipalityProjectionTest<TProjection>
        where TProjection : ConnectedProjection<Municipality.MunicipalityConsumerContext>, new()
    {
        protected ConnectedProjectionTest<Municipality.MunicipalityConsumerContext, TProjection> Sut { get; }

        public MunicipalityProjectionTest()
        {
            Sut = new ConnectedProjectionTest<Municipality.MunicipalityConsumerContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual Municipality.MunicipalityConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<Municipality.MunicipalityConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new Municipality.MunicipalityConsumerContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
