namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using global::AutoFixture;
    using Projections.Legacy.AddressDetailV2;
    using Xunit;

    public class AddressDetailItemV2Tests : AddressLegacyProjectionTest<AddressDetailProjectionsV2>
    {
        private readonly Fixture? _fixture;

        public AddressDetailItemV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithExtendedWkbGeometry());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressDetailItemV2.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    addressDetailItemV2.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    addressDetailItemV2.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    addressDetailItemV2.Status.Should().Be(addressWasMigratedToStreetName.Status);
                    addressDetailItemV2.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    addressDetailItemV2.Position.Should().BeEquivalentTo(addressWasMigratedToStreetName.ExtendedWkbGeometry?.ToByteArray() ?? null);
                    addressDetailItemV2.PositionMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(addressWasMigratedToStreetName.GeometrySpecification);
                    addressDetailItemV2.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasMigratedToStreetName.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressDetailItemV2.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    addressDetailItemV2.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    addressDetailItemV2.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressDetailItemV2.OfficiallyAssigned.Should().BeTrue();
                    addressDetailItemV2.Position.Should().BeEquivalentTo(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(addressWasProposedV2.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(addressWasProposedV2.GeometrySpecification);
                    addressDetailItemV2.Removed.Should().BeFalse();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasProposedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasApproved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var approvedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Current);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasApproved.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejected>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRejected>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(addressWasRejected, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejectedBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRetired>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejectedBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(addressWasRejectedBecauseHouseNumberWasRetired, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(addressWasRejected, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2, retireMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetiredV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseHouseNumberWasRetired()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRetired()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var deregulatedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDeregulated.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, deregulatedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasDeregulated.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.OfficiallyAssigned.Should().BeFalse();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasDeregulated.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRegularized()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var deregulatedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDeregulated.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var regularizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRegularized.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, deregulatedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, regularizedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRegularized.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.OfficiallyAssigned.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRegularized.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>();
            var positionChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPositionWasChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, positionChangedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Position.Should().BeEquivalentTo(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(addressPositionWasChanged.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(addressPositionWasChanged.GeometrySpecification);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressPositionWasChanged.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var positionWasCorrectedV2 = _fixture.Create<AddressPositionWasCorrectedV2>();
            var positionCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, positionWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(positionWasCorrectedV2, positionCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Position.Should().BeEquivalentTo(positionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(positionWasCorrectedV2.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(positionWasCorrectedV2.GeometrySpecification);
                    addressDetailItemV2.VersionTimestamp.Should().Be(positionWasCorrectedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(positionWasCorrectedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRemoved = _fixture.Create<AddressWasRemovedV2>();
            var addressWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedV2>(new Envelope(addressWasRemoved, addressWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Removed.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRemoved.GetHash());
                });
        }

        protected override AddressDetailProjectionsV2 CreateProjection()
            => new AddressDetailProjectionsV2();
    }
}
