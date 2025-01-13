namespace AddressRegistry.Tests.ProjectionTests.WfsV2
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.DataStructures;
    using AddressRegistry.StreetName.Events;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using Projections.Wfs;
    using Projections.Wfs.AddressWfsV2;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;
    using IHouseNumberLabelUpdater = Projections.Wfs.AddressWfsV2.IHouseNumberLabelUpdater;

    public class AddressWfsV2ProjectionTests : AddressWfsItemV2ProjectionTest
    {
        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader;
        private readonly Mock<IHouseNumberLabelUpdater> _houseNumberLabelUpdaterMock;

        public AddressWfsV2ProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize<AddressStatus>(_ => new WithoutUnknownStreetNameAddressStatus());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new InfrastructureCustomization());

            _wkbReader = WKBReaderFactory.Create();
            _houseNumberLabelUpdaterMock = new Mock<IHouseNumberLabelUpdater>();
        }

        protected override AddressWfsV2Projections CreateProjection()
            =>  new AddressWfsV2Projections(_wkbReader, _houseNumberLabelUpdaterMock.Object);

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName_HouseNumber()
        {
            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithBoxNumber(null)
                .WithParentAddressPersistentLocalId(null);

            await Sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.ParentAddressPersistentLocalId.Should().BeNull();
                    addressWfsItem.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressWfsItem.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    addressWfsItem.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    addressWfsItem.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsV2Projections.MapStatus(addressWasMigratedToStreetName.Status));
                    addressWfsItem.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressWasMigratedToStreetName.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressWasMigratedToStreetName.GeometrySpecification));
                    addressWfsItem.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);

                    _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                        It.IsAny<WfsContext>(),
                        It.IsAny<AddressWfsV2Item>(),
                        It.IsAny<CancellationToken>(),
                        true), Times.Once);
                });
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName_HouseNumberWithBoxNumber()
        {
            var houseNumberAddressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithBoxNumber(null)
                .WithParentAddressPersistentLocalId(null)
                .WithNotRemoved()
                .WithStatus(AddressStatus.Proposed);
            var boxNumberAddressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1))
                .WithBoxNumber(_fixture.Create<BoxNumber>())
                .WithAddressGeometry(
                    new AddressGeometry(
                        houseNumberAddressWasMigratedToStreetName.GeometryMethod,
                        houseNumberAddressWasMigratedToStreetName.GeometrySpecification,
                        ExtendedWkbGeometry.CreateEWkb(houseNumberAddressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray()))
                    )
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Proposed);

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberAddressWasMigratedToStreetName, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(boxNumberAddressWasMigratedToStreetName, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var houseNumberAddressWfsItem = await ct.AddressWfsV2Items.FindAsync(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    houseNumberAddressWfsItem.Should().NotBeNull();
                    var boxNumberAddressWfsItem = await ct.AddressWfsV2Items.FindAsync(boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    boxNumberAddressWfsItem.Should().NotBeNull();
                    boxNumberAddressWfsItem!.ParentAddressPersistentLocalId.Should()
                        .Be(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenHouseNumberAddressWasProposedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressWfsItem.ParentAddressPersistentLocalId.Should().BeNull();
                    addressWfsItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    addressWfsItem.BoxNumber.Should().BeNull();
                    addressWfsItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressWasProposedV2.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressWasProposedV2.GeometrySpecification));
                    addressWfsItem.Removed.Should().BeFalse();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);
        }

        [Fact]
        public async Task WhenHouseNumberAddressWasProposedForMunicipalityMerger()
        {
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress();

            await Sut
                .Given(new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(addressWasProposedForMunicipalityMerger, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);
                    addressWfsItem.ParentAddressPersistentLocalId.Should().BeNull();
                    addressWfsItem.HouseNumber.Should().Be(addressWasProposedForMunicipalityMerger.HouseNumber);
                    addressWfsItem.BoxNumber.Should().BeNull();
                    addressWfsItem.PostalCode.Should().Be(addressWasProposedForMunicipalityMerger.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.OfficiallyAssigned.Should().Be(addressWasProposedForMunicipalityMerger.OfficiallyAssigned);
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedForMunicipalityMerger.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressWasProposedForMunicipalityMerger.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressWasProposedForMunicipalityMerger.GeometrySpecification));
                    addressWfsItem.Removed.Should().BeFalse();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasProposedForMunicipalityMerger.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);
        }

        [Fact]
        public async Task WhenBoxNumberAddressWasProposedV2()
        {
            var houseNumberWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberWasMigrated.AddressPersistentLocalId + 1))
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberWasMigrated.AddressPersistentLocalId));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberWasMigrated, new Dictionary<string, object>())),
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressWfsItem.ParentAddressPersistentLocalId.Should().Be(houseNumberWasMigrated.AddressPersistentLocalId);
                    addressWfsItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    addressWfsItem.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    addressWfsItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressWasProposedV2.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressWasProposedV2.GeometrySpecification));
                    addressWfsItem.Removed.Should().BeFalse();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Current));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed()
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

            var addressApprovalWasCorrected = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();
            var correctionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressApprovalWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(addressApprovalWasCorrected, correctionMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
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

            var addressApprovalWasCorrected = _fixture.Create<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();
            var correctionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressApprovalWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(new Envelope(addressApprovalWasCorrected, correctionMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfMunicipalityMerger()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseOfMunicipalityMerger>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>(new Envelope(addressWasRejected, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRejected>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRejected>(new Envelope(addressWasRejected, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(addressWasRetired, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Retired));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRejectedToProposed()
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

            var addressRejectionWasCorrected = _fixture.Create<AddressWasCorrectedFromRejectedToProposed>();
            var correctedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressRejectionWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, rejectedMetadata)),
                    new Envelope<AddressWasCorrectedFromRejectedToProposed>(new Envelope(addressRejectionWasCorrected, correctedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressRejectionWasCorrected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.VersionTimestamp.Should().Be(addressRejectionWasCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasDeregulated.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.OfficiallyAssigned.Should().BeFalse();
                    addressWfsItem.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Current));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRegularized()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var deregulatedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRegularized.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, deregulatedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRegularized.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.OfficiallyAssigned.Should().BeTrue();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
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
            var approvedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();
            var retiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2, retiredMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetiredV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Retired));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfMunicipalityMerger()
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

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfMunicipalityMerger>();
            var retiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>(new Envelope(addressWasRetired, retiredMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Retired));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
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
            var approvedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();
            var retiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired, retiredMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Retired));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
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
            var approvedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();
            var retiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired, retiredMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Retired));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
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

            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();
            var retiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredV2.GetHash() }
            };

            var addressWasCorrectedFromRetiredToCurrent = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();
            var correctedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasCorrectedFromRetiredToCurrent.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2, retiredMetadata)),
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(addressWasCorrectedFromRetiredToCurrent, correctedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetiredV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Current));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(4));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(3));
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasChangedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithParentAddressPersistentLocalId(null)
                .WithPostalCode(new PostalCode("9000"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var boxNumberAddressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithPostalCode(new PostalCode("9000"));
            var boxNumberProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberAddressWasProposedV2.GetHash() }
            };

            var addressPostalCodeWasChangedV2 = _fixture.Create<AddressPostalCodeWasChangedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithBoxNumberPersistentLocalIds(new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) })
                .WithPostalCode(new PostalCode("2000"));
            var postalCodeWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPostalCodeWasChangedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressPostalCodeWasChangedV2>(new Envelope(addressPostalCodeWasChangedV2, postalCodeWasChangedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressPostalCodeWasChangedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    addressWfsItem.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    var boxNumberAddressWfsItem = await ct.AddressWfsV2Items.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberAddressWfsItem.Should().NotBeNull();
                    boxNumberAddressWfsItem!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasChangedV2.PostalCode);
                    boxNumberAddressWfsItem.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithParentAddressPersistentLocalId(null)
                .WithPostalCode(new PostalCode("9000"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var boxNumberAddressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithPostalCode(new PostalCode("9000"));
            var boxNumberProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberAddressWasProposedV2.GetHash() }
            };

            var addressPostalCodeWasCorrectedV2 = _fixture.Create<AddressPostalCodeWasCorrectedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithBoxNumberPersistentLocalIds(new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) })
                .WithPostalCode(new PostalCode("2000"));
            var postalCodeWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPostalCodeWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressPostalCodeWasCorrectedV2>(new Envelope(addressPostalCodeWasCorrectedV2, postalCodeWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    addressWfsItem.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberAddressWfsItem = await ct.AddressWfsV2Items.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberAddressWfsItem.Should().NotBeNull();
                    boxNumberAddressWfsItem!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasCorrectedV2.PostalCode);
                    boxNumberAddressWfsItem.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithParentAddressPersistentLocalId(null)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithHouseNumber(new HouseNumber("101"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var boxNumberAddressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithHouseNumber(new HouseNumber("101"));
            var boxNumberProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberAddressWasProposedV2.GetHash() }
            };

            var addressHouseNumberWasCorrectedV2 = _fixture.Create<AddressHouseNumberWasCorrectedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithBoxNumberPersistentLocalIds(new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) })
                .WithHouseNumber(new HouseNumber("102"));
            var houseNumberWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressHouseNumberWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(addressHouseNumberWasCorrectedV2, houseNumberWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    addressWfsItem.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberAddressWfsItem = await ct.AddressWfsV2Items.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberAddressWfsItem.Should().NotBeNull();
                    boxNumberAddressWfsItem!.HouseNumber.Should().BeEquivalentTo(addressHouseNumberWasCorrectedV2.HouseNumber);
                    boxNumberAddressWfsItem.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(4));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithParentAddressPersistentLocalId(null)
                .WithHouseNumber(new HouseNumber("101"))
                .WithBoxNumber(new BoxNumber("A1"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressBoxNumberWasCorrectedV2 = new AddressBoxNumberWasCorrectedV2(
                new StreetNamePersistentLocalId(2),
                new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId),
                new BoxNumber("1B"));
            ((ISetProvenance)addressBoxNumberWasCorrectedV2).SetProvenance(_fixture.Create<Provenance>());

            var boxNumberWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressBoxNumberWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressBoxNumberWasCorrectedV2>(new Envelope(addressBoxNumberWasCorrectedV2, boxNumberWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);
                    addressWfsItem.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberAddressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    boxNumberAddressWfsItem.Should().NotBeNull();
                    boxNumberAddressWfsItem!.BoxNumber.Should().BeEquivalentTo(addressBoxNumberWasCorrectedV2.BoxNumber);
                    boxNumberAddressWfsItem.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressPositionWasChanged.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressPositionWasChanged.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressPositionWasChanged.GeometrySpecification));
                    addressWfsItem.Position.Should().Be((Point) _wkbReader.Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WithBoxNumber_WhenAddressPositionWasChanged()
        {
            var addressWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithPosition(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

            var boxNumberWasProposed = _fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(addressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasMigrated.AddressPersistentLocalId + 1))
                .WithExtendedWkbGeometry(GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry());

            var houseNumberPositionWasChanged = _fixture.Create<AddressPositionWasChanged>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasMigrated.AddressPersistentLocalId))
                .WithExtendedWkbGeometry(GeometryHelpers.ThirdGmlPointGeometry.ToExtendedWkbGeometry());

            var boxNumberPositionWasChanged = _fixture.Create<AddressPositionWasChanged>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(boxNumberWasProposed.AddressPersistentLocalId))
                .WithExtendedWkbGeometry(GeometryHelpers.ThirdGmlPointGeometry.ToExtendedWkbGeometry());

            var addressWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigrated.GetHash() }
            };

            var boxNumberWasProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberWasProposed.GetHash() }
            };

            var houseNumberPositionWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, houseNumberPositionWasChanged.GetHash() }
            };

            var boxNumberPositionWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberPositionWasChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigrated, addressWasMigratedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberWasProposed, boxNumberWasProposedMetadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(houseNumberPositionWasChanged, houseNumberPositionWasChangedMetadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(boxNumberPositionWasChanged, boxNumberPositionWasChangedMetadata)))
                .Then(async ct =>
                {
                    var houseNumberWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasMigrated.AddressPersistentLocalId);
                    houseNumberWfsItem.Should().NotBeNull();
                    houseNumberWfsItem!.Position.Should().Be((Point)_wkbReader.Read(houseNumberPositionWasChanged.ExtendedWkbGeometry.ToByteArray()));

                    var boxNumberWfsItem = await ct.AddressWfsV2Items.FindAsync(boxNumberWasProposed.AddressPersistentLocalId);
                    boxNumberWfsItem.Should().NotBeNull();
                    boxNumberWfsItem!.Position.Should().Be(houseNumberWfsItem.Position);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(4));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressPositionWasCorrectedV2.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressPositionWasCorrectedV2.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressPositionWasCorrectedV2.GeometrySpecification));
                    addressWfsItem.Position.Should().Be((Point)_wkbReader.Read(addressPositionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.VersionTimestamp.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WithBoxNumber_WhenAddressPositionWasCorrectedV2()
        {
            var addressWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithPosition(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

            var boxNumberWasProposed = _fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(addressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasMigrated.AddressPersistentLocalId + 1))
                .WithExtendedWkbGeometry(GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry());

            var houseNumberPositionWasCorrected = _fixture.Create<AddressPositionWasCorrectedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasMigrated.AddressPersistentLocalId))
                .WithExtendedWkbGeometry(GeometryHelpers.ThirdGmlPointGeometry.ToExtendedWkbGeometry());

            var boxNumberPositionWasCorrected = _fixture.Create<AddressPositionWasCorrectedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(boxNumberWasProposed.AddressPersistentLocalId))
                .WithExtendedWkbGeometry(GeometryHelpers.ThirdGmlPointGeometry.ToExtendedWkbGeometry());

            var addressWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigrated.GetHash() }
            };

            var boxNumberWasProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberWasProposed.GetHash() }
            };

            var houseNumberPositionWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, houseNumberPositionWasCorrected.GetHash() }
            };

            var boxNumberPositionWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberPositionWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigrated, addressWasMigratedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberWasProposed, boxNumberWasProposedMetadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(houseNumberPositionWasCorrected, houseNumberPositionWasChangedMetadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(boxNumberPositionWasCorrected, boxNumberPositionWasChangedMetadata)))
                .Then(async ct =>
                {
                    var houseNumberWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasMigrated.AddressPersistentLocalId);
                    houseNumberWfsItem.Should().NotBeNull();
                    houseNumberWfsItem!.Position.Should().Be((Point)_wkbReader.Read(houseNumberPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()));

                    var boxNumberWfsItem = await ct.AddressWfsV2Items.FindAsync(boxNumberWasProposed.AddressPersistentLocalId);
                    boxNumberWfsItem.Should().NotBeNull();
                    boxNumberWfsItem!.Position.Should().Be(houseNumberWfsItem.Position);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(4));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasReaddressed()
        {
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var boxNumberAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId + 1);

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithBoxNumber(null);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressBoxNumberWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(boxNumberAddressPersistentLocalId);
            var proposedBoxNumberMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressBoxNumberWasProposedV2.GetHash() }
            };

            var readdressedHouseNumber = new ReaddressedAddressData(
                new AddressPersistentLocalId(addressPersistentLocalId + 10),
                addressPersistentLocalId,
                isDestinationNewlyProposed: true,
                AddressStatus.Current,
                new HouseNumber("3"),
                boxNumber: null,
                new PostalCode("9000"),
                new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()),
                sourceIsOfficiallyAssigned: false);

            var readdressedBoxNumber = new ReaddressedAddressData(
                new AddressPersistentLocalId(addressPersistentLocalId + 11),
                boxNumberAddressPersistentLocalId,
                isDestinationNewlyProposed: true,
                AddressStatus.Current,
                new HouseNumber("3"),
                new BoxNumber("A"),
                new PostalCode("9000"),
                new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()),
                sourceIsOfficiallyAssigned: false);

            var addressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
                _fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                readdressedHouseNumber,
                new List<ReaddressedAddressData> { readdressedBoxNumber });
            ((ISetProvenance)addressHouseNumberWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var addressHouseNumberWasReaddressedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressHouseNumberWasReaddressed.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(addressBoxNumberWasProposedV2, proposedBoxNumberMetadata)),
                    new Envelope<AddressHouseNumberWasReaddressed>(new Envelope(addressHouseNumberWasReaddressed, addressHouseNumberWasReaddressedMetadata)))
                .Then(async ct =>
                {
                    var houseNumberItem = await ct.AddressWfsV2Items.FindAsync((int)addressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();

                    houseNumberItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(readdressedHouseNumber.SourceStatus));
                    houseNumberItem.HouseNumber.Should().Be(readdressedHouseNumber.DestinationHouseNumber);
                    houseNumberItem.BoxNumber.Should().Be(null);
                    houseNumberItem.PostalCode.Should().Be(readdressedHouseNumber.SourcePostalCode);
                    houseNumberItem.OfficiallyAssigned.Should().Be(readdressedHouseNumber.SourceIsOfficiallyAssigned);
                    houseNumberItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(readdressedHouseNumber.SourceGeometryMethod));
                    houseNumberItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(readdressedHouseNumber.SourceGeometrySpecification));
                    houseNumberItem.Position.Should().Be((Point)_wkbReader.Read(readdressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray()));
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);

                    var boxNumberItem = await ct.AddressWfsV2Items.FindAsync((int)boxNumberAddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();

                    boxNumberItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(readdressedBoxNumber.SourceStatus));
                    boxNumberItem.HouseNumber.Should().Be(readdressedBoxNumber.DestinationHouseNumber);
                    boxNumberItem.BoxNumber.Should().Be(readdressedBoxNumber.SourceBoxNumber);
                    boxNumberItem.PostalCode.Should().Be(readdressedBoxNumber.SourcePostalCode);
                    boxNumberItem.OfficiallyAssigned.Should().Be(readdressedBoxNumber.SourceIsOfficiallyAssigned);
                    boxNumberItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(readdressedBoxNumber.SourceGeometryMethod));
                    boxNumberItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(readdressedBoxNumber.SourceGeometrySpecification));
                    boxNumberItem.Position.Should().Be((Point)_wkbReader.Read(readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray()));
                    boxNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(4));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
        }

        [Fact]
        public async Task WhenAddressWasProposedBecauseOfReaddress()
        {
            var addressWasProposed = _fixture.Create<AddressWasProposedBecauseOfReaddress>();

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(addressWasProposed, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasProposed.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposed.StreetNamePersistentLocalId);
                    addressWfsItem.HouseNumber.Should().Be(addressWasProposed.HouseNumber);
                    addressWfsItem.BoxNumber.Should().Be(addressWasProposed.BoxNumber);
                    addressWfsItem.PostalCode.Should().Be(addressWasProposed.PostalCode);
                    addressWfsItem.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWfsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposed.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsV2Projections.ConvertGeometryMethodToString(addressWasProposed.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsV2Projections.ConvertGeometrySpecificationToString(addressWasProposed.GeometrySpecification));
                    addressWfsItem.Removed.Should().BeFalse();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasProposed.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(1));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfReaddress()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseOfReaddress>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseOfReaddress>(new Envelope(addressWasRejected, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfReaddress()
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

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfReaddress>();
            var retiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approvedMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfReaddress>(new Envelope(addressWasRetired, retiredMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Retired));
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(3));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Exactly(2));
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
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRemoved.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Removed.Should().BeTrue();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRemovedBecauseStreetNameWasRemoved = _fixture.Create<AddressWasRemovedBecauseStreetNameWasRemoved>();
            var addressWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRemovedBecauseStreetNameWasRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>(new Envelope(addressWasRemovedBecauseStreetNameWasRemoved, addressWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Removed.Should().BeTrue();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRemoved = _fixture.Create<AddressWasRemovedBecauseHouseNumberWasRemoved>();
            var addressWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(addressWasRemoved, addressWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressWasRemoved.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Removed.Should().BeTrue();
                    addressWfsItem.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressRegularizationWasCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var addressWasRegularizedMetData = new Dictionary<string, object>()
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRegularized.GetHash() }
            };

            var addressRegularizationWasCorrected = _fixture.Create<AddressRegularizationWasCorrected>();
            var addressRegularizationWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressRegularizationWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, addressWasRegularizedMetData)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressRegularizationWasCorrected, addressRegularizationWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressRegularizationWasCorrected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(AddressStatus.Current));
                    addressWfsItem.OfficiallyAssigned.Should().BeFalse();
                    addressWfsItem.VersionTimestamp.Should().Be(addressRegularizationWasCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressDeregulationWasCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var addressWasDeregulatedMetData = new Dictionary<string, object>()
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDeregulated.GetHash() }
            };

            var addressDeregulationWasCorrected = _fixture.Create<AddressDeregulationWasCorrected>();
            var addressDeregulationWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressDeregulationWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, addressWasDeregulatedMetData)),
                    new Envelope<AddressDeregulationWasCorrected>(new Envelope(addressDeregulationWasCorrected, addressDeregulationWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(addressDeregulationWasCorrected.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWfsItem.VersionTimestamp.Should().Be(addressDeregulationWasCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Once);
        }

        [Fact]
        public async Task WhenAddressRemovalWasCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var @event = _fixture.Create<AddressRemovalWasCorrected>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressRemovalWasCorrected>(new Envelope(@event, eventMetadata)))
                .Then(async ct =>
                {
                    var expectedGeometry = (Point)_wkbReader.Read(@event.ExtendedWkbGeometry.ToByteArray());

                    var addressWfsItem = await ct.AddressWfsV2Items.FindAsync(@event.AddressPersistentLocalId);
                    addressWfsItem.Should().NotBeNull();

                    addressWfsItem!.Status.Should().Be(AddressWfsV2Projections.MapStatus(@event.Status));
                    addressWfsItem.PostalCode.Should().Be(@event.PostalCode);
                    addressWfsItem.HouseNumber.Should().Be(@event.HouseNumber);
                    addressWfsItem.BoxNumber.Should().Be(@event.BoxNumber);
                    addressWfsItem.Position.Should().BeEquivalentTo(expectedGeometry);
                    addressWfsItem.PositionX.Should().Be(expectedGeometry.X);
                    addressWfsItem.PositionY.Should().Be(expectedGeometry.Y);
                    addressWfsItem.PositionMethod.Should().Be(
                        AddressWfsV2Projections.ConvertGeometryMethodToString(@event.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(
                        AddressWfsV2Projections.ConvertGeometrySpecificationToString(@event.GeometrySpecification));
                    addressWfsItem.OfficiallyAssigned.Should().Be(@event.OfficiallyAssigned);
                    addressWfsItem.Removed.Should().BeFalse();
                    addressWfsItem.ParentAddressPersistentLocalId.Should().Be(@event.ParentPersistentLocalId);

                    addressWfsItem.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Exactly(2));

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var streetNameNamesWereCorrected = _fixture.Create<StreetNameNamesWereCorrected>();
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var streetNameNamesWereChanged = _fixture.Create<StreetNameNamesWereChanged>();
            var streetNameNamesWereChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, streetNameNamesWereChangedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrectedWithOlderTimestamp()
        {
            var provenance = _fixture.Create<Provenance>();
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var streetNameNamesWereCorrected = _fixture.Create<StreetNameNamesWereCorrected>();
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereCorrected.GetHash() }
            };
            ((ISetProvenance) streetNameNamesWereCorrected).SetProvenance(new Provenance(
                provenance.Timestamp.Minus(Duration.FromDays(1)),
                provenance.Application,
                provenance.Reason,
                provenance.Operator,
                provenance.Modification,
                provenance.Organisation
            ));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChangedWithOlderTimestamp()
        {
            var provenance = _fixture.Create<Provenance>();
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var streetNameNamesWereChanged = _fixture.Create<StreetNameNamesWereChanged>();
            var streetNameNamesWereChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereChanged.GetHash() }
            };
            ((ISetProvenance) streetNameNamesWereChanged).SetProvenance(new Provenance(
                provenance.Timestamp.Minus(Duration.FromDays(1)),
                provenance.Application,
                provenance.Reason,
                provenance.Operator,
                provenance.Modification,
                provenance.Organisation
            ));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, streetNameNamesWereChangedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var streetNameHomonymAdditionsWereCorrected = _fixture.Create<StreetNameHomonymAdditionsWereCorrected>();
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameHomonymAdditionsWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(streetNameHomonymAdditionsWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var streetNameHomonymAdditionsWereRemoved = _fixture.Create<StreetNameHomonymAdditionsWereRemoved>();
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameHomonymAdditionsWereRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(streetNameHomonymAdditionsWereRemoved, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsV2Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                });

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                true), Times.Once);

            _houseNumberLabelUpdaterMock.Verify(x => x.UpdateHouseNumberLabels(
                It.IsAny<WfsContext>(),
                It.IsAny<AddressWfsV2Item>(),
                It.IsAny<CancellationToken>(),
                false), Times.Never);
        }
    }
}
