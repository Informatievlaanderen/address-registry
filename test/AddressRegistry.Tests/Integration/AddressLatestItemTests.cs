namespace AddressRegistry.Tests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Options;
    using Projections.Integration.Convertors;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.LatestItem;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;

    public sealed class AddressLatestItemTests : IntegrationProjectionTest<AddressLatestItemProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/adres";
        private readonly Fixture _fixture;

        public AddressLatestItemTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithStatus(AddressStatus.Proposed);
            var geometry = WKBReaderFactory.CreateForLegacy().Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray());

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    expectedLatestItem.ParentPersistentLocalId.Should().Be(addressWasMigratedToStreetName.ParentPersistentLocalId);
                    expectedLatestItem.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    expectedLatestItem.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    expectedLatestItem.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    expectedLatestItem.Status.Should().Be(addressWasMigratedToStreetName.Status);
                    expectedLatestItem.OsloStatus.Should().Be(addressWasMigratedToStreetName.Status.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    expectedLatestItem.PositionMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should().Be(addressWasMigratedToStreetName.GeometrySpecification);
                    expectedLatestItem.OsloPositionSpecification.Should()
                        .Be(addressWasMigratedToStreetName.GeometrySpecification.ToPositieSpecificatie());
                    expectedLatestItem.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasMigratedToStreetName.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                    expectedLatestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedV2()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();

            var position = _fixture.Create<long>();

            var geometry = WKBReaderFactory.CreateForLegacy().Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray());

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.ParentPersistentLocalId.Should().Be(addressWasProposedV2.ParentPersistentLocalId);
                    expectedLatestItem.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    expectedLatestItem.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    expectedLatestItem.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().Be(true);
                    expectedLatestItem.PositionMethod.Should().Be(addressWasProposedV2.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(addressWasProposedV2.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should().Be(addressWasProposedV2.GeometrySpecification);
                    expectedLatestItem.OsloPositionSpecification.Should().Be(addressWasProposedV2.GeometrySpecification.ToPositieSpecificatie());
                    expectedLatestItem.Removed.Should().Be(false);
                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasProposedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    expectedLatestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedForMunicipalityMerger()
        {
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>();

            var geometry = WKBReaderFactory.CreateForLegacy().Read(
                addressWasProposedForMunicipalityMerger.ExtendedWkbGeometry.ToByteArray());

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(addressWasProposedForMunicipalityMerger, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);
                    expectedLatestItem.ParentPersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.ParentPersistentLocalId);
                    expectedLatestItem.HouseNumber.Should().Be(addressWasProposedForMunicipalityMerger.HouseNumber);
                    expectedLatestItem.BoxNumber.Should().Be(addressWasProposedForMunicipalityMerger.BoxNumber);
                    expectedLatestItem.PostalCode.Should().Be(addressWasProposedForMunicipalityMerger.PostalCode);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().Be(addressWasProposedForMunicipalityMerger.OfficiallyAssigned);
                    expectedLatestItem.PositionMethod.Should().Be(addressWasProposedForMunicipalityMerger.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(addressWasProposedForMunicipalityMerger.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should().Be(addressWasProposedForMunicipalityMerger.GeometrySpecification);
                    expectedLatestItem.OsloPositionSpecification.Should().Be(addressWasProposedForMunicipalityMerger.GeometrySpecification.ToPositieSpecificatie());
                    expectedLatestItem.Removed.Should().Be(false);
                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasProposedForMunicipalityMerger.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    expectedLatestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task WhenAddressWasApproved()
        {
            var addressWasApproved = _fixture.Create<AddressWasApproved>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasApproved.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Current.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasApproved.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed()
        {
            var addressWasCorrectedFromApprovedToProposed = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(addressWasCorrectedFromApprovedToProposed, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasCorrectedFromApprovedToProposed.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromApprovedToProposed.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasCorrectedFromApprovedToProposed.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
        {
            var addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected =
                _fixture.Create<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(
                        new Envelope(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected
                            .AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should()
                        .Be($"{Namespace}/{addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should()
                        .Be(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected()
        {
            var addressWasRejected = _fixture.Create<AddressWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasRejected.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRejected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var addressWasRejectedBecauseHouseNumberWasRejected = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(addressWasRejectedBecauseHouseNumberWasRejected,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRejectedBecauseHouseNumberWasRejected.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseHouseNumberWasRejected.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseHouseNumberWasRejected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressWasRejectedBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(addressWasRejectedBecauseHouseNumberWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseHouseNumberWasRetired.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var addressWasRejectedBecauseStreetNameWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRejected>(new Envelope(addressWasRejectedBecauseStreetNameWasRejected,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRejectedBecauseStreetNameWasRejected.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseStreetNameWasRejected.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseStreetNameWasRejected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseStreetNameWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var addressWasRetiredBecauseStreetNameWasRejected = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(addressWasRetiredBecauseStreetNameWasRejected,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRetiredBecauseStreetNameWasRejected.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredBecauseStreetNameWasRejected.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseStreetNameWasRejected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseStreetNameWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressWasRejectedBecauseStreetNameWasRetired = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(addressWasRejectedBecauseStreetNameWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRejectedBecauseStreetNameWasRetired.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseStreetNameWasRetired.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseStreetNameWasRetired.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseStreetNameWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated()
        {
            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasDeregulated.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasDeregulated.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Current.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeFalse();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasDeregulated.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRegularized()
        {
            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRegularized.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRegularized.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRegularized.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2()
        {
            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRetiredV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRetiredV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfMunicipalityMerger()
        {
            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfMunicipalityMerger>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>(new Envelope(addressWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRetired.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetired.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRetired.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseHouseNumberWasRetired()
        {
            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredBecauseHouseNumberWasRetired.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRetired()
        {
            var addressWasRetiredBecauseStreetNameWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(addressWasRetiredBecauseStreetNameWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRetiredBecauseStreetNameWasRetired.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredBecauseStreetNameWasRetired.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseStreetNameWasRetired.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseStreetNameWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
        {
            var addressWasCorrectedFromRetiredToCurrent = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(addressWasCorrectedFromRetiredToCurrent,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasCorrectedFromRetiredToCurrent.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromRetiredToCurrent.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Current.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasCorrectedFromRetiredToCurrent.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasChangedV2()
        {
            var addressPostalCodeWasChangedV2 = _fixture.Create<AddressPostalCodeWasChangedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPostalCodeWasChangedV2>(new Envelope(addressPostalCodeWasChangedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressPostalCodeWasChangedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressPostalCodeWasChangedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeFalse();
                    expectedLatestItem.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressPostalCodeWasChangedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrectedV2()
        {
            var addressPostalCodeWasCorrectedV2 = _fixture.Create<AddressPostalCodeWasCorrectedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPostalCodeWasCorrectedV2>(new Envelope(addressPostalCodeWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressPostalCodeWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeFalse();
                    expectedLatestItem.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressPostalCodeWasCorrectedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2()
        {
            var addressHouseNumberWasCorrectedV2 = _fixture.Create<AddressHouseNumberWasCorrectedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(addressHouseNumberWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressHouseNumberWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeFalse();
                    expectedLatestItem.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressHouseNumberWasCorrectedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrectedV2()
        {
            var addressBoxNumberWasCorrectedV2 = _fixture.Create<AddressBoxNumberWasCorrectedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressBoxNumberWasCorrectedV2>(new Envelope(addressBoxNumberWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressBoxNumberWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeFalse();
                    expectedLatestItem.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressBoxNumberWasCorrectedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged()
        {
            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressPositionWasChanged.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressPositionWasChanged.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeFalse();
                    expectedLatestItem.PositionMethod.Should().Be(addressPositionWasChanged.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(addressPositionWasChanged.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should()
                        .Be(addressPositionWasChanged.GeometrySpecification);
                   expectedLatestItem.OsloPositionSpecification.Should()
                        .Be(addressPositionWasChanged.GeometrySpecification.ToPositieSpecificatie());
                    var geometry = WKBReaderFactory.CreateForLegacy().Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray());
                    expectedLatestItem.Geometry.Should().BeEquivalentTo(geometry);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressPositionWasChanged.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrectedV2()
        {
            var addressPositionWasCorrectedV2 = _fixture.Create<AddressPositionWasCorrectedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(addressPositionWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressPositionWasCorrectedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressPositionWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeFalse();
                    expectedLatestItem.PositionMethod.Should().Be(addressPositionWasCorrectedV2.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(addressPositionWasCorrectedV2.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should()
                        .Be(addressPositionWasCorrectedV2.GeometrySpecification);
                    expectedLatestItem.OsloPositionSpecification.Should()
                        .Be(addressPositionWasCorrectedV2.GeometrySpecification.ToPositieSpecificatie());
                    var geometry = WKBReaderFactory.CreateForLegacy().Read(addressPositionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray());
                    expectedLatestItem.Geometry.Should().BeEquivalentTo(geometry);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressPositionWasCorrectedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasReaddressed()
        {
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var boxNumberAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId + 1);
            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithBoxNumber(null);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var addressBoxNumberWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(boxNumberAddressPersistentLocalId);
            var proposedBoxNumberMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressBoxNumberWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
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
                { AddEventHashPipe.HashMetadataKey, addressHouseNumberWasReaddressed.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(addressBoxNumberWasProposedV2, proposedBoxNumberMetadata)),
                    new Envelope<AddressHouseNumberWasReaddressed>(new Envelope(addressHouseNumberWasReaddressed,
                        addressHouseNumberWasReaddressedMetadata)))
                .Then(async ct =>
                {
                    var houseNumberItem = await ct.AddressLatestItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();

                    houseNumberItem!.Status.Should().Be(readdressedHouseNumber.SourceStatus);
                    houseNumberItem.OsloStatus.Should().Be(readdressedHouseNumber.SourceStatus.Map());
                    houseNumberItem.HouseNumber.Should().Be(readdressedHouseNumber.DestinationHouseNumber);
                    houseNumberItem.BoxNumber.Should().Be(null);
                    houseNumberItem.PostalCode.Should().Be(readdressedHouseNumber.SourcePostalCode);
                    houseNumberItem.OfficiallyAssigned.Should().Be(readdressedHouseNumber.SourceIsOfficiallyAssigned);
                    var houseNumberItemGeometry =
                        WKBReaderFactory.CreateForLegacy().Read(readdressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());
                    houseNumberItem.Geometry.Should().BeEquivalentTo(houseNumberItemGeometry);
                    houseNumberItem.PositionMethod.Should().Be(readdressedHouseNumber.SourceGeometryMethod);
                    houseNumberItem.OsloPositionMethod.Should().Be(readdressedHouseNumber.SourceGeometryMethod.ToPositieGeometrieMethode());
                    houseNumberItem.PositionSpecification.Should().Be(readdressedHouseNumber.SourceGeometrySpecification);
                    houseNumberItem.OsloPositionSpecification.Should().Be(readdressedHouseNumber.SourceGeometrySpecification.ToPositieSpecificatie());
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);

                    houseNumberItem.Namespace.Should().Be(Namespace);
                    houseNumberItem.PuriId.Should().Be($"{Namespace}/{addressHouseNumberWasReaddressed.AddressPersistentLocalId}");
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);


                    var boxNumberItem = await ct.AddressLatestItems.FindAsync(addressBoxNumberWasProposedV2.AddressPersistentLocalId);
                    houseNumberItem.Should().NotBeNull();
                    boxNumberItem!.Status.Should().Be(readdressedBoxNumber.SourceStatus);
                    boxNumberItem.OsloStatus.Should().Be(readdressedBoxNumber.SourceStatus.Map());
                    boxNumberItem.HouseNumber.Should().Be(readdressedBoxNumber.DestinationHouseNumber);
                    boxNumberItem.BoxNumber.Should().Be(readdressedBoxNumber.SourceBoxNumber);
                    boxNumberItem.PostalCode.Should().Be(readdressedBoxNumber.SourcePostalCode);
                    boxNumberItem.OfficiallyAssigned.Should().Be(readdressedBoxNumber.SourceIsOfficiallyAssigned);
                    var boxNumberItemGeometry = WKBReaderFactory.CreateForLegacy().Read(readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray());
                    boxNumberItem.Geometry.Should().BeEquivalentTo(boxNumberItemGeometry);
                    boxNumberItem.PositionMethod.Should().Be(readdressedBoxNumber.SourceGeometryMethod);
                    boxNumberItem.OsloPositionMethod.Should().Be(readdressedBoxNumber.SourceGeometryMethod.ToPositieGeometrieMethode());
                    boxNumberItem.PositionSpecification.Should().Be(readdressedBoxNumber.SourceGeometrySpecification);
                    boxNumberItem.OsloPositionSpecification.Should().Be(readdressedBoxNumber.SourceGeometrySpecification.ToPositieSpecificatie());
                    boxNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);

                    boxNumberItem.Namespace.Should().Be(Namespace);
                    boxNumberItem.PuriId.Should().Be($"{Namespace}/{readdressedBoxNumber.DestinationAddressPersistentLocalId}");
                    boxNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedBecauseOfReaddress()
        {
            var addressWasProposedBecauseOfReaddress = _fixture.Create<AddressWasProposedBecauseOfReaddress>();

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(addressWasProposedBecauseOfReaddress, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasProposedBecauseOfReaddress.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasProposedBecauseOfReaddress.StreetNamePersistentLocalId);
                    expectedLatestItem.ParentPersistentLocalId.Should().Be(addressWasProposedBecauseOfReaddress.ParentPersistentLocalId);
                    expectedLatestItem.HouseNumber.Should().Be(addressWasProposedBecauseOfReaddress.HouseNumber);
                    expectedLatestItem.BoxNumber.Should().Be(addressWasProposedBecauseOfReaddress.BoxNumber);
                    expectedLatestItem.PostalCode.Should().Be(addressWasProposedBecauseOfReaddress.PostalCode);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.PositionMethod.Should().Be(addressWasProposedBecauseOfReaddress.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(addressWasProposedBecauseOfReaddress.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should()
                        .Be(addressWasProposedBecauseOfReaddress.GeometrySpecification);
                   expectedLatestItem.OsloPositionSpecification.Should()
                        .Be(addressWasProposedBecauseOfReaddress.GeometrySpecification.ToPositieSpecificatie());
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasProposedBecauseOfReaddress.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasProposedBecauseOfReaddress.Provenance.Timestamp);

                    var geometry = WKBReaderFactory.CreateForLegacy().Read(addressWasProposedBecauseOfReaddress.ExtendedWkbGeometry.ToByteArray());
                    expectedLatestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfReaddress()
        {
            var addressWasRejectedBecauseOfReaddress = _fixture.Create<AddressWasRejectedBecauseOfReaddress>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseOfReaddress>(new Envelope(addressWasRejectedBecauseOfReaddress, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRejectedBecauseOfReaddress.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasRejectedBecauseOfReaddress.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseOfReaddress.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRejectedBecauseOfReaddress.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfReaddress()
        {
            var addressWasRetiredBecauseOfReaddress = _fixture.Create<AddressWasRetiredBecauseOfReaddress>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfReaddress>(new Envelope(addressWasRetiredBecauseOfReaddress, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRetiredBecauseOfReaddress.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasRetiredBecauseOfReaddress.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseOfReaddress.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRetiredBecauseOfReaddress.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedV2()
        {
            var addressWasRemovedV2 = _fixture.Create<AddressWasRemovedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedV2>(new Envelope(addressWasRemovedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRemovedV2.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(addressWasRemovedV2.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeTrue();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRemovedV2.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRemovedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var addressWasRemovedBecauseStreetNameWasRemoved = _fixture.Create<AddressWasRemovedBecauseStreetNameWasRemoved>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>(new Envelope(addressWasRemovedBecauseStreetNameWasRemoved, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRemovedBecauseStreetNameWasRemoved.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeTrue();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressWasRemovedBecauseHouseNumberWasRemoved = _fixture.Create<AddressWasRemovedBecauseHouseNumberWasRemoved>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(addressWasRemovedBecauseHouseNumberWasRemoved,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasRemovedBecauseHouseNumberWasRemoved.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRemovedBecauseHouseNumberWasRemoved.StreetNamePersistentLocalId);
                    expectedLatestItem.Removed.Should().BeTrue();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasRemovedBecauseHouseNumberWasRemoved.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasRemovedBecauseHouseNumberWasRemoved.Provenance.Timestamp);
                });
        }


        [Fact]
        public async Task WhenAddressWasCorrectedFromRejectedToProposed()
        {
            var addressWasCorrectedFromRejectedToProposed = _fixture.Create<AddressWasCorrectedFromRejectedToProposed>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromRejectedToProposed>(new Envelope(addressWasCorrectedFromRejectedToProposed, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressWasCorrectedFromRejectedToProposed.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromRejectedToProposed.StreetNamePersistentLocalId);
                    expectedLatestItem.Status.Should().Be(AddressStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressWasCorrectedFromRejectedToProposed.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressRegularizationWasCorrected()
        {
            var addressRegularizationWasCorrected = _fixture.Create<AddressRegularizationWasCorrected>();

            var position = _fixture.Create<long>();

            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithStatus(AddressStatus.Proposed)
                .WithOfficiallyAssigned(true);

            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, proposedMetadata)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressRegularizationWasCorrected, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressRegularizationWasCorrected.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressRegularizationWasCorrected.StreetNamePersistentLocalId);
                    expectedLatestItem.OfficiallyAssigned.Should().BeFalse();
                    expectedLatestItem.Status.Should().Be(AddressStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(AddressStatus.Current.Map());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressRegularizationWasCorrected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressRegularizationWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressDeregulationWasCorrected()
        {
            var addressDeregulationWasCorrected = _fixture.Create<AddressDeregulationWasCorrected>();

            var position = _fixture.Create<long>();

            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithStatus(AddressStatus.Proposed)
                .WithOfficiallyAssigned(false);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, proposedMetadata)),
                    new Envelope<AddressDeregulationWasCorrected>(new Envelope(addressDeregulationWasCorrected, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(addressDeregulationWasCorrected.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should()
                        .Be(addressDeregulationWasCorrected.StreetNamePersistentLocalId);
                    expectedLatestItem.OfficiallyAssigned.Should().BeTrue();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{addressDeregulationWasCorrected.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(addressDeregulationWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressRemovalWasCorrected()
        {
            var @event = _fixture.Create<AddressRemovalWasCorrected>()
                .WithStatus(AddressStatus.Current);

            var position = _fixture.Create<long>();

            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithStatus(AddressStatus.Proposed);
            var addressWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, addressWasMigratedMetadata)),
                    new Envelope<AddressRemovalWasCorrected>(new Envelope(@event, eventMetadata)))
                .Then(async ct =>
                {
                    var expectedGeometry = WKBReaderFactory.CreateForLegacy().Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray());

                    var expectedLatestItem =
                        await ct.AddressLatestItems.FindAsync(@event.AddressPersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.StreetNamePersistentLocalId.Should().Be(@event.StreetNamePersistentLocalId);

                    expectedLatestItem.ParentPersistentLocalId.Should().Be(@event.ParentPersistentLocalId);
                    expectedLatestItem.Status.Should().Be(@event.Status);
                    expectedLatestItem.OsloStatus.Should().Be(@event.Status.Map());
                    expectedLatestItem.PostalCode.Should().Be(@event.PostalCode);
                    expectedLatestItem.HouseNumber.Should().Be(@event.HouseNumber);
                    expectedLatestItem.BoxNumber.Should().Be(@event.BoxNumber);
                    expectedLatestItem.Geometry.Should().Be(expectedGeometry);
                    expectedLatestItem.PositionMethod.Should().Be(@event.GeometryMethod);
                    expectedLatestItem.OsloPositionMethod.Should().Be(@event.GeometryMethod.ToPositieGeometrieMethode());
                    expectedLatestItem.PositionSpecification.Should().Be(@event.GeometrySpecification);
                    expectedLatestItem.OsloPositionSpecification.Should().Be(@event.GeometrySpecification.ToPositieSpecificatie());
                    expectedLatestItem.OfficiallyAssigned.Should().Be(@event.OfficiallyAssigned);
                    expectedLatestItem.Removed.Should().BeFalse();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{@event.AddressPersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                });
        }

        protected override AddressLatestItemProjections CreateProjection()
            => new AddressLatestItemProjections(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
