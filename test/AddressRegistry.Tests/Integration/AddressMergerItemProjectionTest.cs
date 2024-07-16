namespace AddressRegistry.Tests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Projections.Integration.Merger;
    using StreetName.Events;
    using Xunit;

    public class AddressMergerItemProjectionTest : IntegrationProjectionTest<AddressMergerItemProjections>
    {
        private readonly Fixture _fixture;

        public AddressMergerItemProjectionTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithValidHouseNumber());
        }

        [Fact]
        public async Task WhenAddressWasProposedForMunicipalityMerger()
        {
            var addressWasProposedBecauseOfMunicipalityMerger = _fixture.Create<AddressWasProposedBecauseOfMunicipalityMerger>();

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfMunicipalityMerger>(new Envelope(
                    addressWasProposedBecauseOfMunicipalityMerger, firstEventMetadata)))
                .Then(async ct =>
                {
                    var expectedItem = await ct.AddressMergerItems.SingleOrDefaultAsync(x =>
                        x.NewPersistentLocalId == addressWasProposedBecauseOfMunicipalityMerger.AddressPersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.NewPersistentLocalId.Should()
                        .Be(addressWasProposedBecauseOfMunicipalityMerger.AddressPersistentLocalId);
                    expectedItem.MergedPersistentLocalId.Should()
                        .Be(addressWasProposedBecauseOfMunicipalityMerger.MergedAddressPersistentLocalId);
                });
        }

        protected override AddressMergerItemProjections CreateProjection() => new();
    }
}
