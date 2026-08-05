namespace AddressRegistry.Tests.ProjectionTests.WfsV3
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wfs;
    using Projections.Wfs.AddressWfsV3;

    public abstract class AddressWfsItemV3ProjectionTest
    {
        protected ConnectedProjectionTest<WfsContext, AddressWfsV3Projections> Sut { get; }

        protected AddressWfsItemV3ProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WfsContext, AddressWfsV3Projections>(CreateContext, CreateProjection);
        }

        protected virtual WfsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WfsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WfsContext(options);
        }

        protected abstract AddressWfsV3Projections CreateProjection();
    }
}
