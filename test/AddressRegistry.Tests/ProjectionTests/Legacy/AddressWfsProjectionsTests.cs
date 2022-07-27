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
            var approvedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, approvedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
        }

        protected override AddressWfsProjections CreateProjection()
            =>  new AddressWfsProjections(_wkbReader);
    }
}
