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
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using NodaTime;
    using Projections.Legacy.AddressListV2;
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

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed()
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

            var addressApprovalWasCorrected = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();
            var correctionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressApprovalWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, metadata2)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(addressApprovalWasCorrected, correctionMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressListItemV2.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressApprovalWasCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
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

            var addressApprovalWasCorrected = _fixture.Create<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();
            var correctionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressApprovalWasCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, metadata2)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(new Envelope(addressApprovalWasCorrected, correctionMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasApproved.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Proposed);
                    addressListItemV2.VersionTimestamp.Should().Be(addressApprovalWasCorrected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressApprovalWasCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRegularized()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRegularized.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRegularized.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRegularized.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDeregulated.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasDeregulated.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2!.Status.Should().Be(AddressStatus.Current);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasDeregulated.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejected>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRejected>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(addressWasRejected, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(addressWasRejectedBecauseHouseNumberWasRetired, rejectedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.GetHash());
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
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
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

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(addressWasRetired, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRetired.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(addressWasRejected, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2()
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

            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2, retireMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRetiredV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseHouseNumberWasRetired()
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

            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRetired()
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

            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetiredBecauseHouseNumberWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2, retireMetadata)),
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(addressWasCorrectedFromRetiredToCurrent, correctedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRetiredV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2!.Status.Should().Be(AddressStatus.Current);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasCorrectedFromRetiredToCurrent.GetHash());
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
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressPostalCodeWasChangedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2!.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    addressListItemV2.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressPostalCodeWasChangedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressListV2.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId));
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
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressPostalCodeWasCorrectedV2>(new Envelope(addressPostalCodeWasCorrectedV2, addressPostalCodeWasCorrectedV2Metadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2!.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    addressListItemV2.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressPostalCodeWasCorrectedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressListV2.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId));
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
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberAddressWasProposedV2, boxNumberProposedMetadata)),
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(addressHouseNumberWasCorrectedV2, addressHouseNumberWasCorrectedV2Metadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2!.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    addressListItemV2.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressHouseNumberWasCorrectedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressListV2.FindAsync(boxNumberAddressWasProposedV2.AddressPersistentLocalId));
                    boxNumberAddressDetailItemV2.Should().NotBeNull();
                    boxNumberAddressDetailItemV2!.HouseNumber.Should().BeEquivalentTo(addressHouseNumberWasCorrectedV2.HouseNumber);
                    boxNumberAddressDetailItemV2.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                    boxNumberAddressDetailItemV2.LastEventHash.Should().Be(addressHouseNumberWasCorrectedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithHouseNumber(new HouseNumber("101"))
                .WithBoxNumber(new BoxNumber("A1"));
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressBoxNumberWasCorrectedV2>(new Envelope(addressBoxNumberWasCorrectedV2, addressHouseNumberWasCorrectedV2Metadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2!.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);
                    addressListItemV2.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressBoxNumberWasCorrectedV2.GetHash());

                    var boxNumberAddressDetailItemV2 = (await ct.AddressListV2.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId));
                    boxNumberAddressDetailItemV2.Should().NotBeNull();
                    boxNumberAddressDetailItemV2!.BoxNumber.Should().BeEquivalentTo(addressBoxNumberWasCorrectedV2.BoxNumber);
                    boxNumberAddressDetailItemV2.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);
                    boxNumberAddressDetailItemV2.LastEventHash.Should().Be(addressBoxNumberWasCorrectedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, positionChangedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressPositionWasChanged.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressPositionWasChanged.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrectedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(positionWasCorrectedV2, positionCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(positionWasCorrectedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(positionWasCorrectedV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(positionWasCorrectedV2.GetHash());
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
                    var houseNumberItem = await ct.AddressListV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();

                    houseNumberItem!.Status.Should().Be(readdressedHouseNumber.SourceStatus);
                    houseNumberItem.HouseNumber.Should().Be(readdressedHouseNumber.DestinationHouseNumber);
                    houseNumberItem.BoxNumber.Should().Be(readdressedHouseNumber.SourceBoxNumber);
                    houseNumberItem.PostalCode.Should().Be(readdressedHouseNumber.SourcePostalCode);
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);
                    houseNumberItem.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());

                    var boxNumberItem = await ct.AddressListV2.FindAsync(addressBoxNumberWasProposedV2.AddressPersistentLocalId);
                    boxNumberItem.Should().NotBeNull();

                    boxNumberItem!.Status.Should().Be(readdressedBoxNumber.SourceStatus);
                    boxNumberItem.HouseNumber.Should().Be(readdressedBoxNumber.DestinationHouseNumber);
                    boxNumberItem.BoxNumber.Should().Be(readdressedBoxNumber.SourceBoxNumber);
                    boxNumberItem.PostalCode.Should().Be(readdressedBoxNumber.SourcePostalCode);
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
                    var expectedListItem = (await ct.AddressListV2.FindAsync(addressWasProposed.AddressPersistentLocalId));
                    expectedListItem.Should().NotBeNull();
                    expectedListItem.StreetNamePersistentLocalId.Should().Be(addressWasProposed.StreetNamePersistentLocalId);
                    expectedListItem.HouseNumber.Should().Be(addressWasProposed.HouseNumber);
                    expectedListItem.BoxNumber.Should().Be(addressWasProposed.BoxNumber);
                    expectedListItem.PostalCode.Should().Be(addressWasProposed.PostalCode);
                    expectedListItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedListItem.Removed.Should().BeFalse();
                    expectedListItem.VersionTimestamp.Should().Be(addressWasProposed.Provenance.Timestamp);
                    expectedListItem.LastEventHash.Should().Be(addressWasProposed.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfReaddress()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseOfReaddress>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRejectedBecauseOfReaddress>(new Envelope(addressWasRejected, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRejected.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Rejected);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfReaddress()
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

            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfReaddress>();
            var retireMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRetired.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, approveMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfReaddress>(new Envelope(addressWasRetired, retireMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRetired.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Retired);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRetired.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRemoved = _fixture.Create<AddressWasRemovedV2>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRemovedV2>(new Envelope(addressWasRemoved, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Removed.Should().BeTrue();
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRemoved.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRemovedBecauseStreetNameWasRemoved = _fixture.Create<AddressWasRemovedBecauseStreetNameWasRemoved>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRemovedBecauseStreetNameWasRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>(new Envelope(addressWasRemovedBecauseStreetNameWasRemoved, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Removed.Should().BeTrue();
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRemoved = _fixture.Create<AddressWasRemovedBecauseHouseNumberWasRemoved>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(addressWasRemoved, metadata2)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasRemoved.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Removed.Should().BeTrue();
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasRemoved.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasRemoved.GetHash());
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
                    var addressDetailItemV2 = (await ct.AddressListV2.FindAsync(addressRejectionWasCorrected.AddressPersistentLocalId));
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
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();
            var metadata2 = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized, metadata2)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressWasCorrectedFromRegularized, correctedFromRegularizedMetaData)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasCorrectedFromRegularized.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.Status.Should().Be(AddressStatus.Current);
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasCorrectedFromRegularized.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasCorrectedFromRegularized.GetHash());
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromDeregularized()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var addressWasDeregulatedMetadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated, addressWasDeregulatedMetadata)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressWasCorrectedFromRegularized, correctedFromRegularizedMetaData)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasCorrectedFromRegularized.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasCorrectedFromRegularized.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(addressWasCorrectedFromRegularized.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(streetNameNamesWereCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrectedWithOlderTimestamp()
        {
            var provenance = _fixture.Create<Provenance>();
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(streetNameNamesWereCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(streetNameHomonymAdditionsWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(streetNameHomonymAdditionsWereCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
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
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)),
                    new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(streetNameHomonymAdditionsWereRemoved, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var addressListItemV2 = (await ct.AddressListV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId));
                    addressListItemV2.Should().NotBeNull();
                    addressListItemV2.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                    addressListItemV2.LastEventHash.Should().Be(streetNameHomonymAdditionsWereRemoved.GetHash());
                });
        }

        protected override AddressListProjectionsV2 CreateProjection()
            => new AddressListProjectionsV2();
    }
}
