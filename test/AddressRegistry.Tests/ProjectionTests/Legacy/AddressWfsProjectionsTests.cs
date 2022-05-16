namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
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

        protected override AddressWfsProjections CreateProjection()
            =>  new AddressWfsProjections(_wkbReader);
    }
}
