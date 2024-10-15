namespace AddressRegistry.Tests.ProjectionTests.WmsV2
{
    using System.Collections.Generic;
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
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using Projections.Wms;
    using Projections.Wms.AddressWmsItemV2;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public class AddressWmsItemV2ProjectionTests : AddressWmsItemV2ProjectionTest
    {
        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader;

        public AddressWmsItemV2ProjectionTests()
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
        }

        protected override AddressWmsItemV2Projections CreateProjection()
            =>  new AddressWmsItemV2Projections(_wkbReader);

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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.ParentAddressPersistentLocalId.Should().BeNull();
                    addressWmsItem.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressWmsItem.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    addressWmsItem.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    addressWmsItem.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxes);
                    addressWmsItem.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    addressWmsItem.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(addressWasMigratedToStreetName.Status));
                    addressWmsItem.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    addressWmsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressWasMigratedToStreetName.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressWasMigratedToStreetName.GeometrySpecification));
                    addressWmsItem.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName_HouseNumberWithBoxNumberOnSamePosition()
        {
            var houseNumberAddressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithBoxNumber(null)
                .WithParentAddressPersistentLocalId(null)
                .WithNotRemoved();
            var boxNumberAddressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1))
                .WithBoxNumber(_fixture.Create<BoxNumber>())
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberAddressWasMigratedToStreetName, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(boxNumberAddressWasMigratedToStreetName, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var houseNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    houseNumberAddressWmsItem.Should().NotBeNull();
                    houseNumberAddressWmsItem!.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
                    var boxNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.Should().NotBeNull();
                    boxNumberAddressWmsItem!.ParentAddressPersistentLocalId.Should()
                        .Be(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
                });
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName_HouseNumberWithBoxNumberOnDifferentPosition()
        {
            var houseNumberAddressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithBoxNumber(null)
                .WithParentAddressPersistentLocalId(null)
                .WithNotRemoved();
            var boxNumberAddressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithPosition(new ExtendedWkbGeometry(GeometryHelpers.ExampleExtendedWkb))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1))
                .WithBoxNumber(_fixture.Create<BoxNumber>())
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberAddressWasMigratedToStreetName, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(boxNumberAddressWasMigratedToStreetName, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var houseNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    houseNumberAddressWmsItem.Should().NotBeNull();
                    houseNumberAddressWmsItem!.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxes);
                    var boxNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.Should().NotBeNull();
                    boxNumberAddressWmsItem!.ParentAddressPersistentLocalId.Should()
                        .Be(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressWmsItem.ParentAddressPersistentLocalId.Should().BeNull();
                    addressWmsItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    addressWmsItem.BoxNumber.Should().BeNull();
                    addressWmsItem.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxes);
                    addressWmsItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    addressWmsItem.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWmsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressWasProposedV2.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressWasProposedV2.GeometrySpecification));
                    addressWmsItem.Removed.Should().BeFalse();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);
                    addressWmsItem.ParentAddressPersistentLocalId.Should().BeNull();
                    addressWmsItem.HouseNumber.Should().Be(addressWasProposedForMunicipalityMerger.HouseNumber);
                    addressWmsItem.BoxNumber.Should().BeNull();
                    addressWmsItem.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxes);
                    addressWmsItem.PostalCode.Should().Be(addressWasProposedForMunicipalityMerger.PostalCode);
                    addressWmsItem.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.OfficiallyAssigned.Should().Be(addressWasProposedForMunicipalityMerger.OfficiallyAssigned);
                    addressWmsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedForMunicipalityMerger.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressWasProposedForMunicipalityMerger.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressWasProposedForMunicipalityMerger.GeometrySpecification));
                    addressWmsItem.Removed.Should().BeFalse();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasProposedForMunicipalityMerger.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressWmsItem.ParentAddressPersistentLocalId.Should().Be(houseNumberWasMigrated.AddressPersistentLocalId);
                    addressWmsItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    addressWmsItem.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    addressWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
                    addressWmsItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    addressWmsItem.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWmsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressWasProposedV2.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressWasProposedV2.GeometrySpecification));
                    addressWmsItem.Removed.Should().BeFalse();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Current));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Retired));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressRejectionWasCorrected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.VersionTimestamp.Should().Be(addressRejectionWasCorrected.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasDeregulated.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.OfficiallyAssigned.Should().BeFalse();
                    addressWmsItem.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Current));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetiredV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Retired));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Retired));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Retired));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Retired));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetiredV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Current));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressPostalCodeWasChangedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    addressWmsItem.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    var boxNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.Should().NotBeNull();
                    boxNumberAddressWmsItem!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasChangedV2.PostalCode);
                    boxNumberAddressWmsItem.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    addressWmsItem.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.Should().NotBeNull();
                    boxNumberAddressWmsItem!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasCorrectedV2.PostalCode);
                    boxNumberAddressWmsItem.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    addressWmsItem.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.Should().NotBeNull();
                    boxNumberAddressWmsItem!.HouseNumber.Should().BeEquivalentTo(addressHouseNumberWasCorrectedV2.HouseNumber);
                    boxNumberAddressWmsItem.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);
                    addressWmsItem.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberAddressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    boxNumberAddressWmsItem.Should().NotBeNull();
                    boxNumberAddressWmsItem!.BoxNumber.Should().BeEquivalentTo(addressBoxNumberWasCorrectedV2.BoxNumber);
                    boxNumberAddressWmsItem.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressPositionWasChanged.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressPositionWasChanged.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressPositionWasChanged.GeometrySpecification));
                    addressWmsItem.Position.Should().Be((Point) _wkbReader.Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
                });
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
                    var houseNumberWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasMigrated.AddressPersistentLocalId);
                    houseNumberWmsItem.Should().NotBeNull();
                    houseNumberWmsItem!.Position.Should().Be((Point)_wkbReader.Read(houseNumberPositionWasChanged.ExtendedWkbGeometry.ToByteArray()));
                    houseNumberWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);

                    var boxNumberWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberWasProposed.AddressPersistentLocalId);
                    boxNumberWmsItem.Should().NotBeNull();
                    boxNumberWmsItem!.Position.Should().Be(houseNumberWmsItem.Position);
                    boxNumberWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressPositionWasCorrectedV2.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressPositionWasCorrectedV2.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressPositionWasCorrectedV2.GeometrySpecification));
                    addressWmsItem.Position.Should().Be((Point)_wkbReader.Read(addressPositionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.VersionTimestamp.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
                });
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
                    var houseNumberWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasMigrated.AddressPersistentLocalId);
                    houseNumberWmsItem.Should().NotBeNull();
                    houseNumberWmsItem!.Position.Should().Be((Point)_wkbReader.Read(houseNumberPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    houseNumberWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);

                    var boxNumberWmsItem = await ct.AddressWmsItemsV2.FindAsync(boxNumberWasProposed.AddressPersistentLocalId);
                    boxNumberWmsItem.Should().NotBeNull();
                    boxNumberWmsItem!.Position.Should().Be(houseNumberWmsItem.Position);
                    boxNumberWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
                });
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
                    var houseNumberItem = await ct.AddressWmsItemsV2.FindAsync((int)addressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();

                    houseNumberItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(readdressedHouseNumber.SourceStatus));
                    houseNumberItem.HouseNumber.Should().Be(readdressedHouseNumber.DestinationHouseNumber);
                    houseNumberItem.BoxNumber.Should().Be(null);
                    houseNumberItem.PostalCode.Should().Be(readdressedHouseNumber.SourcePostalCode);
                    houseNumberItem.OfficiallyAssigned.Should().Be(readdressedHouseNumber.SourceIsOfficiallyAssigned);
                    houseNumberItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(readdressedHouseNumber.SourceGeometryMethod));
                    houseNumberItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(readdressedHouseNumber.SourceGeometrySpecification));
                    houseNumberItem.Position.Should().Be((Point)_wkbReader.Read(readdressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray()));
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);

                    var boxNumberItem = await ct.AddressWmsItemsV2.FindAsync((int)boxNumberAddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();

                    boxNumberItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(readdressedBoxNumber.SourceStatus));
                    boxNumberItem.HouseNumber.Should().Be(readdressedBoxNumber.DestinationHouseNumber);
                    boxNumberItem.BoxNumber.Should().Be(readdressedBoxNumber.SourceBoxNumber);
                    boxNumberItem.PostalCode.Should().Be(readdressedBoxNumber.SourcePostalCode);
                    boxNumberItem.OfficiallyAssigned.Should().Be(readdressedBoxNumber.SourceIsOfficiallyAssigned);
                    boxNumberItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(readdressedBoxNumber.SourceGeometryMethod));
                    boxNumberItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(readdressedBoxNumber.SourceGeometrySpecification));
                    boxNumberItem.Position.Should().Be((Point)_wkbReader.Read(readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray()));
                    boxNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedBecauseOfReaddress()
        {
            var addressWasProposed = _fixture.Create<AddressWasProposedBecauseOfReaddress>();

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(addressWasProposed, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasProposed.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposed.StreetNamePersistentLocalId);
                    addressWmsItem.HouseNumber.Should().Be(addressWasProposed.HouseNumber);
                    addressWmsItem.BoxNumber.Should().Be(addressWasProposed.BoxNumber);
                    addressWmsItem.LabelType.Should().Be(WmsAddressLabelType.BoxNumberOrHouseNumberWithBoxesOnSamePosition);
                    addressWmsItem.PostalCode.Should().Be(addressWasProposed.PostalCode);
                    addressWmsItem.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Proposed));
                    addressWmsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWmsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposed.ExtendedWkbGeometry.ToByteArray()));
                    addressWmsItem.PositionMethod.Should().Be(AddressWmsItemV2Projections.ConvertGeometryMethodToString(addressWasProposed.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(addressWasProposed.GeometrySpecification));
                    addressWmsItem.Removed.Should().BeFalse();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasProposed.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Rejected));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Retired));
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRemoved.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Removed.Should().BeTrue();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Removed.Should().BeTrue();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressWasRemoved.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Removed.Should().BeTrue();
                    addressWmsItem.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressRegularizationWasCorrected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(AddressStatus.Current));
                    addressWmsItem.OfficiallyAssigned.Should().BeFalse();
                    addressWmsItem.VersionTimestamp.Should().Be(addressRegularizationWasCorrected.Provenance.Timestamp);
                });
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
                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(addressDeregulationWasCorrected.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem.OfficiallyAssigned.Should().BeTrue();
                    addressWmsItem.VersionTimestamp.Should().Be(addressDeregulationWasCorrected.Provenance.Timestamp);
                });
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

                    var addressWmsItem = await ct.AddressWmsItemsV2.FindAsync(@event.AddressPersistentLocalId);
                    addressWmsItem.Should().NotBeNull();

                    addressWmsItem!.Status.Should().Be(AddressWmsItemV2Projections.MapStatus(@event.Status));
                    addressWmsItem.PostalCode.Should().Be(@event.PostalCode);
                    addressWmsItem.HouseNumber.Should().Be(@event.HouseNumber);
                    addressWmsItem.BoxNumber.Should().Be(@event.BoxNumber);
                    addressWmsItem.Position.Should().BeEquivalentTo(expectedGeometry);
                    addressWmsItem.PositionX.Should().Be(expectedGeometry.X);
                    addressWmsItem.PositionY.Should().Be(expectedGeometry.Y);
                    addressWmsItem.PositionMethod.Should().Be(
                        AddressWmsItemV2Projections.ConvertGeometryMethodToString(@event.GeometryMethod));
                    addressWmsItem.PositionSpecification.Should().Be(
                        AddressWmsItemV2Projections.ConvertGeometrySpecificationToString(@event.GeometrySpecification));
                    addressWmsItem.OfficiallyAssigned.Should().Be(@event.OfficiallyAssigned);
                    addressWmsItem.Removed.Should().BeFalse();
                    addressWmsItem.ParentAddressPersistentLocalId.Should().Be(@event.ParentPersistentLocalId);

                    addressWmsItem.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                });
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
                    var item = (await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                });
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
                    var item = (await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                });
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
                    var item = (await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });
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
                    var item = (await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                });
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
                    var item = (await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });
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
                    var item = (await ct.AddressWmsItemsV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                });
        }
    }
}
