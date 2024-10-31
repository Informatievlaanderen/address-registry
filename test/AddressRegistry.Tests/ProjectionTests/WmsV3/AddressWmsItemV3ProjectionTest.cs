namespace AddressRegistry.Tests.ProjectionTests.WmsV3
{
    using System;
    using AddressRegistry.Projections.Wms;
    using AddressRegistry.Projections.Wms.AddressWmsItemV3;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;

    public abstract class AddressWmsItemV3ProjectionTest
    {
        protected ConnectedProjectionTest<WmsContext, AddressWmsItemV3Projections> Sut { get; }

        public AddressWmsItemV3ProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WmsContext, AddressWmsItemV3Projections>(CreateContext, CreateProjection);
        }

        protected virtual WmsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WmsContext(options);
        }

        protected abstract AddressWmsItemV3Projections CreateProjection();

    }
}
