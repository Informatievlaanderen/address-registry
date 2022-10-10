namespace AddressRegistry.Tests.ProjectionTests.Legacy.Wms
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wms;
    using Projections.Wms.AddressWmsItem;

    public abstract class AddressWmsItemProjectionTest
    {
        protected ConnectedProjectionTest<WmsContext, AddressWmsItemProjections> Sut { get; }

        public AddressWmsItemProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WmsContext, AddressWmsItemProjections>(CreateContext, CreateProjection);
        }

        protected virtual WmsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WmsContext(options);
        }

        protected abstract AddressWmsItemProjections CreateProjection();

    }
}
