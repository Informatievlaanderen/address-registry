namespace AddressRegistry.Tests.ProjectionTests.WmsV4
{
    using System;
    using AddressRegistry.Projections.Wms;
    using AddressRegistry.Projections.Wms.AddressWmsItemV4;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;

    public abstract class AddressWmsItemV4ProjectionTest
    {
        protected ConnectedProjectionTest<WmsContext, AddressWmsItemV4Projections> Sut { get; }

        public AddressWmsItemV4ProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WmsContext, AddressWmsItemV4Projections>(CreateContext, CreateProjection);
        }

        protected virtual WmsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WmsContext(options);
        }

        protected abstract AddressWmsItemV4Projections CreateProjection();

    }
}
