namespace AddressRegistry.Tests.ProjectionTests.WmsV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wms;
    using Projections.Wms.AddressWmsItemV2;

    public abstract class AddressWmsItemV2ProjectionTest
    {
        protected ConnectedProjectionTest<WmsContext, AddressWmsItemV2Projections> Sut { get; }

        public AddressWmsItemV2ProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WmsContext, AddressWmsItemV2Projections>(CreateContext, CreateProjection);
        }

        protected virtual WmsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WmsContext(options);
        }

        protected abstract AddressWmsItemV2Projections CreateProjection();

    }
}
