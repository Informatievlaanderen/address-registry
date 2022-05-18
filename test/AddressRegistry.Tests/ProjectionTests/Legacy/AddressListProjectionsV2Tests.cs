namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using global::AutoFixture;
    using Projections.Legacy.AddressListV2;
    using StreetName;
    using StreetName.Events;
    using Xunit;

    public class AddressListProjectionsV2Tests : AddressLegacyProjectionTest<AddressListProjectionsV2>
    {
        private readonly Fixture? _fixture;

        public AddressListProjectionsV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() }
            };

            await Sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, metadata)))
                .Then(async ct =>
                {
                    var expectedListItem = (await ct.AddressListV2.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId));
                    expectedListItem.Should().NotBeNull();
                    expectedListItem.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    expectedListItem.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    expectedListItem.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    expectedListItem.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    expectedListItem.Status.Should().Be(addressWasMigratedToStreetName.Status);
                    expectedListItem.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    expectedListItem.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                    expectedListItem.LastEventHash.Should().Be(addressWasMigratedToStreetName.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedListItem = (await ct.AddressListV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    expectedListItem.Should().NotBeNull();
                    expectedListItem.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    expectedListItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    expectedListItem.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    expectedListItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    expectedListItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedListItem.Removed.Should().BeFalse();
                    expectedListItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    expectedListItem.LastEventHash.Should().Be(addressWasProposedV2.GetHash());
                });
        }


        [Fact]
        public async Task WhenAddressWasApproved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Current);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasApproved.GetHash());
                });
        }

        protected override AddressListProjectionsV2 CreateProjection()
            => new AddressListProjectionsV2();
    }
}
