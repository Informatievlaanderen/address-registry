namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wfs;
    using Projections.Wfs.AddressWfs;

    public abstract class AddressWfsProjectionTest
    {
        protected ConnectedProjectionTest<WfsContext, AddressWfsProjections> Sut { get; }

        public AddressWfsProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WfsContext, AddressWfsProjections>(CreateContext, CreateProjection);
        }

        protected virtual WfsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WfsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WfsContext(options);
        }

        protected abstract AddressWfsProjections CreateProjection();

    }
}
