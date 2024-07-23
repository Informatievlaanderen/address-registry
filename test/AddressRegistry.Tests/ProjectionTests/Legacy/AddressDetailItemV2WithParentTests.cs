namespace AddressRegistry.Tests.ProjectionTests.Legacy
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
    using NodaTime;
    using Projections.Legacy.AddressDetailV2WithParent;
    using Xunit;

    public class AddressDetailItemV2WithParentTests : AddressLegacyProjectionTest<AddressDetailProjectionsV2WithParent>
    {
        private readonly Fixture? _fixture;

        public AddressDetailItemV2WithParentTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new WithValidHouseNumber());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressDetailItemV2.ParentAddressPersistentLocalId.Should().Be(addressWasMigratedToStreetName.ParentPersistentLocalId);
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    addressDetailItemV2.ParentAddressPersistentLocalId.Should().Be(addressWasProposedV2.ParentPersistentLocalId);
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
        public async Task WhenAddressWasProposedForMunicipalityMerger()
        {
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedForMunicipalityMerger.GetHash() }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(addressWasProposedForMunicipalityMerger, metadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.StreetNamePersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);
                    addressDetailItemV2.ParentAddressPersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.ParentPersistentLocalId);
                    addressDetailItemV2.HouseNumber.Should().Be(addressWasProposedForMunicipalityMerger.HouseNumber);
                    addressDetailItemV2.BoxNumber.Should().Be(addressWasProposedForMunicipalityMerger.BoxNumber);
                    addressDetailItemV2.PostalCode.Should().Be(addressWasProposedForMunicipalityMerger.PostalCode);
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressDetailItemV2.OfficiallyAssigned.Should().Be(addressWasProposedForMunicipalityMerger.OfficiallyAssigned);
                    addressDetailItemV2.Position.Should().BeEquivalentTo(addressWasProposedForMunicipalityMerger.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(addressWasProposedForMunicipalityMerger.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(addressWasProposedForMunicipalityMerger.GeometrySpecification);
                    addressDetailItemV2.Removed.Should().BeFalse();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasProposedForMunicipalityMerger.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Current);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasApproved.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressApprovalWasCorrected.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressApprovalWasCorrected.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.Status.Should().Be(AddressStatus.Rejected);
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejected.AddressPersistentLocalId));
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
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

            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var rejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(addressWasRetired, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetired.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejected.AddressPersistentLocalId));
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetiredV2.GetHash());
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
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfMunicipalityMerger>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>(new Envelope(addressWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetired.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.Status.Should().Be(AddressStatus.Current);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasCorrectedFromRetiredToCurrent.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasDeregulated.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.OfficiallyAssigned.Should().BeFalse();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Current);
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRegularized.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.OfficiallyAssigned.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRegularized.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasChangedV2.PostalCode);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressPostalCodeWasChangedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId));
                    boxNumberAddressDetailItemV2.Should().NotBeNull();
                    boxNumberAddressDetailItemV2!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasChangedV2.PostalCode);
                    boxNumberAddressDetailItemV2.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                    boxNumberAddressDetailItemV2.LastEventHash.Should().Be(addressPostalCodeWasChangedV2.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasCorrectedV2.PostalCode);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressPostalCodeWasCorrectedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId));
                    boxNumberAddressDetailItemV2.Should().NotBeNull();
                    boxNumberAddressDetailItemV2!.PostalCode.Should().BeEquivalentTo(addressPostalCodeWasCorrectedV2.PostalCode);
                    boxNumberAddressDetailItemV2.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                    boxNumberAddressDetailItemV2.LastEventHash.Should().Be(addressPostalCodeWasCorrectedV2.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.HouseNumber.Should().BeEquivalentTo(addressHouseNumberWasCorrectedV2.HouseNumber);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressHouseNumberWasCorrectedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId));
                    boxNumberAddressDetailItemV2.Should().NotBeNull();
                    boxNumberAddressDetailItemV2!.HouseNumber.Should().BeEquivalentTo(addressHouseNumberWasCorrectedV2.HouseNumber);
                    boxNumberAddressDetailItemV2.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                    boxNumberAddressDetailItemV2.LastEventHash.Should().Be(addressHouseNumberWasCorrectedV2.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Position.Should().BeEquivalentTo(positionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(positionWasCorrectedV2.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(positionWasCorrectedV2.GeometrySpecification);
                    addressDetailItemV2.VersionTimestamp.Should().Be(positionWasCorrectedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(positionWasCorrectedV2.GetHash());
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
                    var houseNumberItem = await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();

                    houseNumberItem!.Status.Should().Be(readdressedHouseNumber.SourceStatus);
                    houseNumberItem.HouseNumber.Should().Be(readdressedHouseNumber.DestinationHouseNumber);
                    houseNumberItem.BoxNumber.Should().Be(null);
                    houseNumberItem.PostalCode.Should().Be(readdressedHouseNumber.SourcePostalCode);
                    houseNumberItem.OfficiallyAssigned.Should().Be(readdressedHouseNumber.SourceIsOfficiallyAssigned);
                    houseNumberItem.Position.Should().BeEquivalentTo(readdressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());
                    houseNumberItem.PositionMethod.Should().Be(readdressedHouseNumber.SourceGeometryMethod);
                    houseNumberItem.PositionSpecification.Should().Be(readdressedHouseNumber.SourceGeometrySpecification);
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);
                    houseNumberItem.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());

                    var boxNumberItem = await ct.AddressDetailV2WithParent.FindAsync(addressBoxNumberWasProposedV2.AddressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();

                    boxNumberItem!.Status.Should().Be(readdressedBoxNumber.SourceStatus);
                    boxNumberItem.HouseNumber.Should().Be(readdressedBoxNumber.DestinationHouseNumber);
                    boxNumberItem.BoxNumber.Should().Be(readdressedBoxNumber.SourceBoxNumber);
                    boxNumberItem.PostalCode.Should().Be(readdressedBoxNumber.SourcePostalCode);
                    boxNumberItem.OfficiallyAssigned.Should().Be(readdressedBoxNumber.SourceIsOfficiallyAssigned);
                    boxNumberItem.Position.Should().BeEquivalentTo(readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray());
                    boxNumberItem.PositionMethod.Should().Be(readdressedBoxNumber.SourceGeometryMethod);
                    boxNumberItem.PositionSpecification.Should().Be(readdressedBoxNumber.SourceGeometrySpecification);
                    boxNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);
                    boxNumberItem.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedBecauseOfReaddress()
        {
            var addressWasProposed = _fixture.Create<AddressWasProposedBecauseOfReaddress>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposed.GetHash() }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(addressWasProposed, metadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposed.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.StreetNamePersistentLocalId.Should().Be(addressWasProposed.StreetNamePersistentLocalId);
                    addressDetailItemV2.ParentAddressPersistentLocalId.Should().Be(addressWasProposed.ParentPersistentLocalId);
                    addressDetailItemV2.HouseNumber.Should().Be(addressWasProposed.HouseNumber);
                    addressDetailItemV2.BoxNumber.Should().Be(addressWasProposed.BoxNumber);
                    addressDetailItemV2.PostalCode.Should().Be(addressWasProposed.PostalCode);
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressDetailItemV2.OfficiallyAssigned.Should().BeTrue();
                    addressDetailItemV2.Position.Should().BeEquivalentTo(addressWasProposed.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(addressWasProposed.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(addressWasProposed.GeometrySpecification);
                    addressDetailItemV2.Removed.Should().BeFalse();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasProposed.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasProposed.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
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
            var approveMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasApproved.GetHash() }
            };

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfReaddress>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfReaddress>(new Envelope(addressWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRetired.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRetired.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Removed.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRemoved.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Removed.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Removed.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasRemoved.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressRejectionWasCorrected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressRejectionWasCorrected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressRejectionWasCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRegularized()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var regularizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRegularized.GetHash() }
            };

            var addressWasCorrectedFromRegularized = _fixture.Create<AddressRegularizationWasCorrected>();
            var correctedFromRegularizedMetaData = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasCorrectedFromRegularized.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, regularizedMetadata)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressWasCorrectedFromRegularized, correctedFromRegularizedMetaData)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasCorrectedFromRegularized.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.Status.Should().Be(AddressStatus.Current);
                    addressDetailItemV2.OfficiallyAssigned.Should().BeFalse();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasCorrectedFromRegularized.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasCorrectedFromRegularized.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromDeregularized()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var regularizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDeregulated.GetHash() }
            };

            var addressDeregulationWasCorrected = _fixture.Create<AddressDeregulationWasCorrected>();
            var correctedFromRegularizedMetaData = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressDeregulationWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, regularizedMetadata)),
                    new Envelope<AddressDeregulationWasCorrected>(new Envelope(addressDeregulationWasCorrected, correctedFromRegularizedMetaData)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressDeregulationWasCorrected.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.OfficiallyAssigned.Should().BeTrue();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressDeregulationWasCorrected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressDeregulationWasCorrected.GetHash());
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
                    var addressDetailItemV2 = await ct.AddressDetailV2WithParent.FindAsync(@event.AddressPersistentLocalId);
                    addressDetailItemV2.Should().NotBeNull();

                    addressDetailItemV2!.ParentAddressPersistentLocalId.Should().Be(@event.ParentPersistentLocalId);
                    addressDetailItemV2.Status.Should().Be(@event.Status);
                    addressDetailItemV2.PostalCode.Should().Be(@event.PostalCode);
                    addressDetailItemV2.HouseNumber.Should().Be(@event.HouseNumber);
                    addressDetailItemV2.BoxNumber.Should().Be(@event.BoxNumber);
                    addressDetailItemV2.Position.Should().BeEquivalentTo(@event.ExtendedWkbGeometry.ToByteArray());
                    addressDetailItemV2.PositionMethod.Should().Be(@event.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(@event.GeometrySpecification);
                    addressDetailItemV2.OfficiallyAssigned.Should().Be(@event.OfficiallyAssigned);
                    addressDetailItemV2.Removed.Should().BeFalse();

                    addressDetailItemV2.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(@event.GetHash());
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
            var namesCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, namesCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(streetNameNamesWereCorrected.GetHash());
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
            var namesChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, namesChangedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(streetNameNamesWereChanged.GetHash());
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
            ((ISetProvenance) streetNameNamesWereCorrected).SetProvenance(new Provenance(
                provenance.Timestamp.Minus(Duration.FromDays(1)),
                provenance.Application,
                provenance.Reason,
                provenance.Operator,
                provenance.Modification,
                provenance.Organisation
            ));
            var namesCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, namesCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(streetNameNamesWereCorrected.GetHash());
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
            ((ISetProvenance) streetNameNamesWereChanged).SetProvenance(new Provenance(
                provenance.Timestamp.Minus(Duration.FromDays(1)),
                provenance.Application,
                provenance.Reason,
                provenance.Operator,
                provenance.Modification,
                provenance.Organisation
            ));
            var namesChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, namesChangedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(streetNameNamesWereChanged.GetHash());
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
            var namesCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameHomonymAdditionsWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(streetNameHomonymAdditionsWereCorrected, namesCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(streetNameHomonymAdditionsWereCorrected.GetHash());
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
            var namesCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameHomonymAdditionsWereRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(streetNameHomonymAdditionsWereRemoved, namesCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(streetNameHomonymAdditionsWereRemoved.GetHash());
                });
        }

        protected override AddressDetailProjectionsV2WithParent CreateProjection()
            => new AddressDetailProjectionsV2WithParent();
    }
}
