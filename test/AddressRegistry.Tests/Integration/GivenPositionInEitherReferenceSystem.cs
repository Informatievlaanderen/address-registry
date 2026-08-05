namespace AddressRegistry.Tests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Options;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.LatestItemV2;
    using StreetName.Events;
    using Xunit;

    /// <summary>
    /// The PostGIS <c>geometry</c> column holds the reference system the event store persisted the
    /// position in, so a row carries its own <c>ST_SRID</c> and views can branch on it while the
    /// conversion runs. See ADR 0004.
    /// </summary>
    public sealed class GivenPositionInEitherReferenceSystem : IntegrationProjectionTest<AddressLatestItemProjectionsV2>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/adres";

        private static Fixture CreateFixture(ICustomization geometry)
        {
            var fixture = new Fixture();
            fixture.Customize(new InfrastructureCustomization());
            fixture.Customize(new WithValidHouseNumber());
            fixture.Customize(new WithValidBoxNumber());
            fixture.Customize(geometry);
            fixture.Customize(new WithFixedAddressPersistentLocalId());
            fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            return fixture;
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72)]
        [InlineData(SystemReferenceId.SridLambert2008)]
        public async Task WhenAddressWasProposedV2_ThenTheGeometryKeepsTheEventsSrid(int expectedSrid)
        {
            var fixture = CreateFixture(expectedSrid == SystemReferenceId.SridLambert2008
                ? new WithExtendedWkbGeometryLambert2008()
                : new WithExtendedWkbGeometry());

            var addressWasProposedV2 = fixture.Create<AddressWasProposedV2>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, fixture.Create<long>() }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var latestItem = await ct.AddressLatestItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    latestItem.Should().NotBeNull();
                    latestItem!.Geometry.Should().NotBeNull();

                    // Npgsql writes this SRID into the geometry column, which is what ST_SRID reads back.
                    latestItem.Geometry!.SRID.Should().Be(expectedSrid);
                });
        }

        protected override AddressLatestItemProjectionsV2 CreateProjection()
            => new AddressLatestItemProjectionsV2(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
