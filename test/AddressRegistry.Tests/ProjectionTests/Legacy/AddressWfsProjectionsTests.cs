namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using global::AutoFixture;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Projections.Wfs.AddressWfs;
    using StreetName;
    using StreetName.Events;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public class AddressWfsProjectionsTests : AddressWfsProjectionTest
    {
        private readonly Fixture? _fixture;
        private readonly WKBReader _wkbReader;

        public AddressWfsProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize<AddressStatus>(c => new WithoutUnknownStreetNameAddressStatus());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _wkbReader = WKBReaderFactory.CreateForLegacy();
        }

        [Fact]
        public async Task AddressWasMigratedToStreetName()
        {
            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>();

            await Sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = (await ct.AddressWfsItems.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId));
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressWfsItem.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    addressWfsItem.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    addressWfsItem.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsProjections.MapStatus(addressWasMigratedToStreetName.Status));
                    addressWfsItem.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsProjections.ConvertGeometryMethodToString(addressWasMigratedToStreetName.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsProjections.ConvertGeometrySpecificationToString(addressWasMigratedToStreetName.GeometrySpecification));
                    addressWfsItem.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                });
        }


        [Fact]
        public async Task AddressWasProposedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = (await ct.AddressWfsItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressWfsItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    addressWfsItem.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    addressWfsItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWfsItem.Position.Should().BeNull();
                    addressWfsItem.PositionMethod.Should().BeNull();
                    addressWfsItem.PositionSpecification.Should().BeNull();
                    addressWfsItem.Removed.Should().BeFalse();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Current));
                    item.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetired()
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Retired));
                    item.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Retired));
                    item.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Retired));
                    item.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasDeregulated.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.OfficiallyAssigned.Should().BeFalse();
                    item.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRegularized.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.OfficiallyAssigned.Should().BeTrue();
                    item.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressPositionWasChanged.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.PositionMethod.Should().Be(AddressWfsProjections.ConvertGeometryMethodToString(addressPositionWasChanged.GeometryMethod));
                    item.PositionSpecification = AddressWfsProjections.ConvertGeometrySpecificationToString(addressPositionWasChanged.GeometrySpecification);
                    item.Position = (Point) _wkbReader.Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray());
                    item.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
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

            var addressPositionWasCorrectedV2 = _fixture.Create<AddressPositionWasCorrectedV2>();
            var positionCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPositionWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(addressPositionWasCorrectedV2, positionCorrectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressPositionWasCorrectedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.PositionMethod.Should().Be(AddressWfsProjections.ConvertGeometryMethodToString(addressPositionWasCorrectedV2.GeometryMethod));
                    item.PositionSpecification = AddressWfsProjections.ConvertGeometrySpecificationToString(addressPositionWasCorrectedV2.GeometrySpecification);
                    item.Position = (Point)_wkbReader.Read(addressPositionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray());
                    item.VersionTimestamp.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
                });
        }

        protected override AddressWfsProjections CreateProjection()
            =>  new AddressWfsProjections(_wkbReader);
    }
}
