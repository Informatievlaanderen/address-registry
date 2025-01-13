namespace AddressRegistry.Tests.ProjectionTests.WfsV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wfs;
    using Projections.Wfs.AddressWfsV2;

    public abstract class AddressWfsItemV2ProjectionTest
    {
        protected ConnectedProjectionTest<WfsContext, AddressWfsV2Projections> Sut { get; }

        protected AddressWfsItemV2ProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WfsContext, AddressWfsV2Projections>(CreateContext, CreateProjection);
        }

        protected virtual WfsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WfsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WfsContext(options);
        }

        protected abstract AddressWfsV2Projections CreateProjection();
    }
}
