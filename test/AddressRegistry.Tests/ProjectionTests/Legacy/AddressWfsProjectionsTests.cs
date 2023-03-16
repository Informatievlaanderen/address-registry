namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Extensions;
    using FluentAssertions;
    using global::AutoFixture;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using Projections.Wfs.AddressWfs;
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
            _fixture.Customize(new WithValidHouseNumber());

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
                    addressWfsItem.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray()));
                    addressWfsItem.PositionMethod.Should().Be(AddressWfsProjections.ConvertGeometryMethodToString(addressWasProposedV2.GeometryMethod));
                    addressWfsItem.PositionSpecification.Should().Be(AddressWfsProjections.ConvertGeometrySpecificationToString(addressWasProposedV2.GeometrySpecification));
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Proposed));
                    item.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Proposed));
                    item.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
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
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRejected>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRejected>(new Envelope(addressWasRejected, metadata2)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(addressWasRejected, metadata2)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Retired));
                    item.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
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

            var addressWasRetired = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(addressWasRetired, rejectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRetired.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Rejected));
                    item.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
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
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
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

            var addressWasCorrectedFromRetiredToCurrent = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();
            var correctedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasCorrectedFromRetiredToCurrent.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2, retireMetadata)),
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(addressWasCorrectedFromRetiredToCurrent, correctedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Current));
                    item.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
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
                    item!.OfficiallyAssigned.Should().BeFalse();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Current));
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
        public async Task WhenAddressPostalCodeWasChangedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithPostalCode(new PostalCode("9000"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var boxNumberAddressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithPostalCode(new PostalCode("9000"));
            var boxNumberProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberAddressWasProposedV2.GetHash() }
            };

            var addressPostalCodeWasChangedV2 = _fixture.Create<AddressPostalCodeWasChangedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithBoxNumberPersistentLocalIds(new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) })
                .WithPostalCode(new PostalCode("2000"));
            var addressPostalCodeWasChangedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPostalCodeWasChangedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressPostalCodeWasChangedV2>(new Envelope(addressPostalCodeWasChangedV2, addressPostalCodeWasChangedV2Metadata)))
                .Then(async ct =>
                {
                    var item = await ct.AddressWfsItems.FindAsync(addressPostalCodeWasChangedV2.AddressPersistentLocalId);
                    item.Should().NotBeNull();
                    item!.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    item.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    var boxNumberItem = await ct.AddressWfsItems.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();
                    boxNumberItem!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasChangedV2.PostalCode);
                    boxNumberItem.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithPostalCode(new PostalCode("9000"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var boxNumberAddressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithPostalCode(new PostalCode("9000"));
            var boxNumberProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberAddressWasProposedV2.GetHash() }
            };

            var addressPostalCodeWasCorrectedV2 = _fixture.Create<AddressPostalCodeWasCorrectedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithBoxNumberPersistentLocalIds(new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) })
                .WithPostalCode(new PostalCode("2000"));
            var addressPostalCodeWasCorrectedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPostalCodeWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressPostalCodeWasCorrectedV2>(new Envelope(addressPostalCodeWasCorrectedV2, addressPostalCodeWasCorrectedV2Metadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item!.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    item.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberItem = await ct.AddressWfsItems.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();
                    boxNumberItem!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasCorrectedV2.PostalCode);
                    boxNumberItem.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithHouseNumber(new HouseNumber("101"));
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var boxNumberAddressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithHouseNumber(new HouseNumber("101"));
            var boxNumberProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberAddressWasProposedV2.GetHash() }
            };

            var addressHouseNumberWasCorrectedV2 = _fixture.Create<AddressHouseNumberWasCorrectedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithBoxNumberPersistentLocalIds(new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) })
                .WithHouseNumber(new HouseNumber("102"));
            var addressHouseNumberWasCorrectedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressHouseNumberWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(addressHouseNumberWasCorrectedV2, addressHouseNumberWasCorrectedV2Metadata)))
                .Then(async ct =>
                {
                    var item = await ct.AddressWfsItems.FindAsync(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);
                    item.Should().NotBeNull();
                    item!.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    item.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberItem = await ct.AddressWfsItems.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();
                    boxNumberItem!.HouseNumber.Should().BeEquivalentTo(addressHouseNumberWasCorrectedV2.HouseNumber);
                    boxNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
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

            var addressHouseNumberWasCorrectedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressBoxNumberWasCorrectedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressBoxNumberWasCorrectedV2>(new Envelope(addressBoxNumberWasCorrectedV2, addressHouseNumberWasCorrectedV2Metadata)))
                .Then(async ct =>
                {
                    var item = await ct.AddressWfsItems.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    item.Should().NotBeNull();
                    item!.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);
                    item.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);

                    var boxNumberItem = await ct.AddressWfsItems.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();
                    boxNumberItem!.BoxNumber.Should().BeEquivalentTo(addressBoxNumberWasCorrectedV2.BoxNumber);
                    boxNumberItem.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);
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
                    item.PositionSpecification.Should().Be(AddressWfsProjections.ConvertGeometrySpecificationToString(addressPositionWasChanged.GeometrySpecification));
                    item.Position.Should().Be((Point) _wkbReader.Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray()));
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
                    item.PositionSpecification.Should().Be(AddressWfsProjections.ConvertGeometrySpecificationToString(addressPositionWasCorrectedV2.GeometrySpecification));
                    item.Position.Should().Be((Point)_wkbReader.Read(addressPositionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray()));
                    item.VersionTimestamp.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Removed.Should().BeTrue();
                    item.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Removed.Should().BeTrue();
                    item.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Removed.Should().BeTrue();
                    item.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
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
            var addressRejectionWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressRejectionWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, rejectedMetadata)),
                    new Envelope<AddressWasCorrectedFromRejectedToProposed>(new Envelope(addressRejectionWasCorrected, addressRejectionWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressRejectionWasCorrected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.Status.Should().Be(AdresStatus.Voorgesteld.ToString());
                    item.VersionTimestamp.Should().Be(addressRejectionWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRegularization()
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

            var addressWasCorrectedFromRegularized = _fixture.Create<AddressRegularizationWasCorrected>();
            var correctedFromRegularizedMetaData = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasCorrectedFromRegularized.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, deregulatedMetadata)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressWasCorrectedFromRegularized, correctedFromRegularizedMetaData)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasCorrectedFromRegularized.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item!.OfficiallyAssigned.Should().BeFalse();
                    item.Status.Should().Be(AddressWfsProjections.MapStatus(AddressStatus.Current));
                    item.VersionTimestamp.Should().Be(addressWasCorrectedFromRegularized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromDeregulation()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var addressWasRegularizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRegularized.GetHash() }
            };

            var addressDeregulationWasCorrected = _fixture.Create<AddressDeregulationWasCorrected>();
            var addressDeregulationWasCorrectedMetaData = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressDeregulationWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, addressWasRegularizedMetadata)),
                    new Envelope<AddressDeregulationWasCorrected>(new Envelope(addressDeregulationWasCorrected, addressDeregulationWasCorrectedMetaData)))
                .Then(async ct =>
                {
                    var item = (await ct.AddressWfsItems.FindAsync(addressDeregulationWasCorrected.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item!.OfficiallyAssigned.Should().BeTrue();
                    item.VersionTimestamp.Should().Be(addressDeregulationWasCorrected.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
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
                    var item = (await ct.AddressWfsItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    item.Should().NotBeNull();
                    item.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                });
        }

        protected override AddressWfsProjections CreateProjection()
            =>  new AddressWfsProjections(_wkbReader);
    }
}
