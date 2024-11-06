namespace AddressRegistry.Tests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Address.Events;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Integration.Convertors;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.Version;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;

    public sealed class AddressVersionTests : IntegrationProjectionTest<AddressVersionProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/adres";
        private readonly Fixture _fixture;
        private readonly Mock<IEventsRepository> _eventsRepositoryMock;

        public AddressVersionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithFixedAddressId());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _eventsRepositoryMock = new Mock<IEventsRepository>();
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
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, metadata)))
                .Then(async ct =>
                {
                    var addressVersion =
                        await ct.AddressVersions.FindAsync(position, addressWasMigratedToStreetName.AddressPersistentLocalId);
                    addressVersion.Should().NotBeNull();
                    addressVersion!.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressVersion.ParentPersistentLocalId.Should().Be(addressWasMigratedToStreetName.ParentPersistentLocalId);
                    addressVersion.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    addressVersion.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    addressVersion.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    addressVersion.Status.Should().Be(addressWasMigratedToStreetName.Status);
                    addressVersion.OsloStatus.Should().Be(addressWasMigratedToStreetName.Status.Map());
                    addressVersion.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    addressVersion.PositionMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod);
                    addressVersion.OsloPositionMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod.ToPositieGeometrieMethode());
                    addressVersion.PositionSpecification.Should().Be(addressWasMigratedToStreetName.GeometrySpecification);
                    addressVersion.OsloPositionSpecification.Should()
                        .Be(addressWasMigratedToStreetName.GeometrySpecification.ToPositieSpecificatie());
                    addressVersion.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    addressVersion.Namespace.Should().Be(Namespace);
                    addressVersion.PuriId.Should().Be($"{Namespace}/{addressWasMigratedToStreetName.AddressPersistentLocalId}");
                    addressVersion.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                    addressVersion.CreatedOnTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                    addressVersion.Geometry.Should().BeEquivalentTo(geometry);
                    addressVersion.Type.Should().Be("EventName");
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
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position, addressWasProposedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    expectedVersion.ParentPersistentLocalId.Should().Be(addressWasProposedV2.ParentPersistentLocalId);
                    expectedVersion.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    expectedVersion.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    expectedVersion.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.OfficiallyAssigned.Should().Be(true);
                    expectedVersion.PositionMethod.Should().Be(addressWasProposedV2.GeometryMethod);
                    expectedVersion.OsloPositionMethod.Should().Be(addressWasProposedV2.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should().Be(addressWasProposedV2.GeometrySpecification);
                    expectedVersion.OsloPositionSpecification.Should().Be(addressWasProposedV2.GeometrySpecification.ToPositieSpecificatie());
                    expectedVersion.Removed.Should().Be(false);
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasProposedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    expectedVersion.CreatedOnTimestamp.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    expectedVersion.Geometry.Should().BeEquivalentTo(geometry);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedForMunicipalityMerger()
        {
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>();

            var geometry = WKBReaderFactory.CreateForLegacy().Read(addressWasProposedForMunicipalityMerger.ExtendedWkbGeometry.ToByteArray());

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(addressWasProposedForMunicipalityMerger, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position, addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);
                    expectedVersion.ParentPersistentLocalId.Should().Be(addressWasProposedForMunicipalityMerger.ParentPersistentLocalId);
                    expectedVersion.HouseNumber.Should().Be(addressWasProposedForMunicipalityMerger.HouseNumber);
                    expectedVersion.BoxNumber.Should().Be(addressWasProposedForMunicipalityMerger.BoxNumber);
                    expectedVersion.PostalCode.Should().Be(addressWasProposedForMunicipalityMerger.PostalCode);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.OfficiallyAssigned.Should().Be(addressWasProposedForMunicipalityMerger.OfficiallyAssigned);
                    expectedVersion.PositionMethod.Should().Be(addressWasProposedForMunicipalityMerger.GeometryMethod);
                    expectedVersion.OsloPositionMethod.Should().Be(addressWasProposedForMunicipalityMerger.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should().Be(addressWasProposedForMunicipalityMerger.GeometrySpecification);
                    expectedVersion.OsloPositionSpecification.Should().Be(addressWasProposedForMunicipalityMerger.GeometrySpecification.ToPositieSpecificatie());
                    expectedVersion.Removed.Should().Be(false);
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasProposedForMunicipalityMerger.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    expectedVersion.CreatedOnTimestamp.Should().Be(addressWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    expectedVersion.Geometry.Should().BeEquivalentTo(geometry);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasApproved()
        {
            var addressWasApproved = _fixture.Create<AddressWasApproved>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasApproved.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1},
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasApproved>(new Envelope(addressWasApproved, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasApproved.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasApproved.StreetNamePersistentLocalId);
                    expectedVersion.ParentPersistentLocalId.Should().Be(addressWasProposedV2.ParentPersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Current);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Current.Map());

                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasApproved.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasApproved.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed()
        {
            var addressWasCorrectedFromApprovedToProposed = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasCorrectedFromApprovedToProposed.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1},
                { Envelope.EventNameMetadataKey, "EventName"},
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(addressWasCorrectedFromApprovedToProposed, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasCorrectedFromApprovedToProposed.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromApprovedToProposed.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasCorrectedFromApprovedToProposed.AddressPersistentLocalId}");

                    expectedVersion.VersionTimestamp.Should().Be(addressWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
        {
            var addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected =
                _fixture.Create<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1},
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(
                        new Envelope(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected
                            .AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should()
                        .Be($"{Namespace}/{addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should()
                        .Be(addressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected()
        {
            var addressWasRejected = _fixture.Create<AddressWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejected.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejected>(new Envelope(addressWasRejected, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasRejected.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfMunicipalityMerger()
        {
            var addressWasRejected = _fixture.Create<AddressWasRejectedBecauseOfMunicipalityMerger>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejected.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>(new Envelope(addressWasRejected, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasRejected.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejected.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var addressWasRejectedBecauseHouseNumberWasRejected = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejectedBecauseHouseNumberWasRejected.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(addressWasRejectedBecauseHouseNumberWasRejected,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejectedBecauseHouseNumberWasRejected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseHouseNumberWasRejected.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseHouseNumberWasRejected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRejected.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressWasRejectedBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(addressWasRejectedBecauseHouseNumberWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseHouseNumberWasRetired.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseHouseNumberWasRetired.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejectedBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var addressWasRejectedBecauseStreetNameWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejectedBecauseStreetNameWasRejected.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRejected>(new Envelope(addressWasRejectedBecauseStreetNameWasRejected,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejectedBecauseStreetNameWasRejected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseStreetNameWasRejected.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseStreetNameWasRejected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejectedBecauseStreetNameWasRejected.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var addressWasRetiredBecauseStreetNameWasRejected = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRetiredBecauseStreetNameWasRejected.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(addressWasRetiredBecauseStreetNameWasRejected,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRetiredBecauseStreetNameWasRejected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredBecauseStreetNameWasRejected.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseStreetNameWasRejected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRetiredBecauseStreetNameWasRejected.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressWasRejectedBecauseStreetNameWasRetired = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejectedBecauseStreetNameWasRetired.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(addressWasRejectedBecauseStreetNameWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejectedBecauseStreetNameWasRetired.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRejectedBecauseStreetNameWasRetired.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseStreetNameWasRetired.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejectedBecauseStreetNameWasRetired.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated()
        {
            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasDeregulated.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasDeregulated>(new Envelope(addressWasDeregulated,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasDeregulated.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasDeregulated.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Current);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Current.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeFalse();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasDeregulated.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasDeregulated.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRegularized()
        {
            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRegularized.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRegularized>(new Envelope(addressWasRegularized,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRegularized.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRegularized.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRegularized.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRegularized.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2()
        {
            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRetiredV2.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredV2>(new Envelope(addressWasRetiredV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRetiredV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredV2.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRetiredV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRetiredV2.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfMunicipalityMerger()
        {
            var addressWasRetired = _fixture.Create<AddressWasRetiredBecauseOfMunicipalityMerger>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRetired.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>(new Envelope(addressWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRetired.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetired.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRetired.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRetired.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseHouseNumberWasRetired()
        {
            var addressWasRetiredBecauseHouseNumberWasRetired = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(addressWasRetiredBecauseHouseNumberWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredBecauseHouseNumberWasRetired.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseHouseNumberWasRetired.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRetiredBecauseHouseNumberWasRetired.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRetired()
        {
            var addressWasRetiredBecauseStreetNameWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRetiredBecauseStreetNameWasRetired.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(addressWasRetiredBecauseStreetNameWasRetired,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRetiredBecauseStreetNameWasRetired.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRetiredBecauseStreetNameWasRetired.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseStreetNameWasRetired.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRetiredBecauseStreetNameWasRetired.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
        {
            var addressWasCorrectedFromRetiredToCurrent = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasCorrectedFromRetiredToCurrent.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(addressWasCorrectedFromRetiredToCurrent,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasCorrectedFromRetiredToCurrent.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromRetiredToCurrent.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Current);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Current.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasCorrectedFromRetiredToCurrent.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                    expectedVersion.Type.Should().Be("EventName");

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasChangedV2()
        {
            var boxNumberPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPostalCodeWasChangedV2 = _fixture.Create<AddressPostalCodeWasChangedV2>()
                .WithBoxNumberPersistentLocalIds(
                    new List<AddressPersistentLocalId>
                    {
                        boxNumberPersistentLocalId
                    });
            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressPostalCodeWasChangedV2.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            var boxNumberWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(boxNumberPersistentLocalId);
            var boxNumberMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberWasProposedV2, boxNumberMetadata)),
                    new Envelope<AddressPostalCodeWasChangedV2>(new Envelope(addressPostalCodeWasChangedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressPostalCodeWasChangedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressPostalCodeWasChangedV2.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressPostalCodeWasChangedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    var expectedBoxNumberVersion =
                        await ct.AddressVersions.FindAsync(position + 1, (int)boxNumberPersistentLocalId);
                    expectedBoxNumberVersion.Should().NotBeNull();
                    expectedBoxNumberVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressPostalCodeWasChangedV2.StreetNamePersistentLocalId);
                    expectedBoxNumberVersion.Removed.Should().BeFalse();
                    expectedBoxNumberVersion.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    expectedBoxNumberVersion.Type.Should().Be("EventName");
                    expectedBoxNumberVersion.Namespace.Should().Be(Namespace);
                    expectedBoxNumberVersion.PuriId.Should().Be($"{Namespace}/{boxNumberPersistentLocalId}");
                    expectedBoxNumberVersion.VersionTimestamp.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrectedV2()
        {
            var houseNumberPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var boxNumberPersistentLocalId = new AddressPersistentLocalId(houseNumberPersistentLocalId + 1);
            var addressPostalCodeWasCorrectedV2 = _fixture.Create<AddressPostalCodeWasCorrectedV2>()
                .WithAddressPersistentLocalId(houseNumberPersistentLocalId)
                .WithBoxNumberPersistentLocalIds(new List<AddressPersistentLocalId>()
                {
                    boxNumberPersistentLocalId
                });

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var boxNumberWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(boxNumberPersistentLocalId);
            var boxNumberMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberWasProposedV2, boxNumberMetadata)),
                    new Envelope<AddressPostalCodeWasCorrectedV2>(new Envelope(addressPostalCodeWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position, addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressPostalCodeWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressPostalCodeWasCorrectedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    var expectedBoxNumberVersion =
                        await ct.AddressVersions.FindAsync(position, (int)boxNumberPersistentLocalId);
                    expectedBoxNumberVersion.Should().NotBeNull();
                    expectedBoxNumberVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressPostalCodeWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedBoxNumberVersion.Removed.Should().BeFalse();
                    expectedBoxNumberVersion.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    expectedBoxNumberVersion.Type.Should().Be("EventName");
                    expectedBoxNumberVersion.Namespace.Should().Be(Namespace);
                    expectedBoxNumberVersion.PuriId.Should().Be($"{Namespace}/{boxNumberPersistentLocalId}");
                    expectedBoxNumberVersion.VersionTimestamp.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2()
        {
            var houseNumberPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var boxNumberPersistentLocalId = houseNumberPersistentLocalId + 1;
            var addressHouseNumberWasCorrectedV2 = _fixture.Create<AddressHouseNumberWasCorrectedV2>()
                .WithBoxNumberPersistentLocalIds(new List<AddressPersistentLocalId>()
                {
                    new(boxNumberPersistentLocalId)
                });

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);

            var boxNumberWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(boxNumberPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var proposedBoxNumberMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, boxNumberWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberWasProposedV2, proposedBoxNumberMetadata)),
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(addressHouseNumberWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 2, addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressHouseNumberWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressHouseNumberWasCorrectedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrectedV2()
        {
            var addressBoxNumberWasCorrectedV2 = _fixture.Create<AddressBoxNumberWasCorrectedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressBoxNumberWasCorrectedV2>(new Envelope(addressBoxNumberWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressBoxNumberWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressBoxNumberWasCorrectedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged()
        {
            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressPositionWasChanged.AddressPersistentLocalId)
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressPositionWasChanged.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressPositionWasChanged.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.PositionMethod.Should().Be(addressPositionWasChanged.GeometryMethod);
                    expectedVersion.OsloPositionMethod.Should().Be(addressPositionWasChanged.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should()
                        .Be(addressPositionWasChanged.GeometrySpecification);
                    expectedVersion.OsloPositionSpecification.Should()
                        .Be(addressPositionWasChanged.GeometrySpecification.ToPositieSpecificatie());
                    var geometry = WKBReaderFactory.CreateForLegacy().Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray());
                    expectedVersion.Geometry.Should().BeEquivalentTo(geometry);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressPositionWasChanged.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressPositionWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrectedV2()
        {
            var addressPositionWasCorrectedV2 = _fixture.Create<AddressPositionWasCorrectedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressPositionWasCorrectedV2.AddressPersistentLocalId)
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(addressPositionWasCorrectedV2,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressPositionWasCorrectedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressPositionWasCorrectedV2.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.PositionMethod.Should().Be(addressPositionWasCorrectedV2.GeometryMethod);
                    expectedVersion.OsloPositionMethod.Should().Be(addressPositionWasCorrectedV2.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should()
                        .Be(addressPositionWasCorrectedV2.GeometrySpecification);
                    expectedVersion.OsloPositionSpecification.Should()
                        .Be(addressPositionWasCorrectedV2.GeometrySpecification.ToPositieSpecificatie());
                    var geometry = WKBReaderFactory.CreateForLegacy().Read(addressPositionWasCorrectedV2.ExtendedWkbGeometry.ToByteArray());
                    expectedVersion.Geometry.Should().BeEquivalentTo(geometry);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressPositionWasCorrectedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasReaddressed()
        {
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var boxNumberAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId + 1);
            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithBoxNumber(null);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var addressBoxNumberWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(boxNumberAddressPersistentLocalId);
            var proposedBoxNumberMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressBoxNumberWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
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
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasProposedV2>(new Envelope(addressBoxNumberWasProposedV2, proposedBoxNumberMetadata)),
                    new Envelope<AddressHouseNumberWasReaddressed>(new Envelope(addressHouseNumberWasReaddressed,
                        addressHouseNumberWasReaddressedMetadata)))
                .Then(async ct =>
                {
                    var houseNumberItem = await ct.AddressVersions.FindAsync(position + 1, addressWasProposedV2.AddressPersistentLocalId);
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
                    houseNumberItem.Type.Should().Be("EventName");
                    houseNumberItem.Namespace.Should().Be(Namespace);
                    houseNumberItem.PuriId.Should().Be($"{Namespace}/{addressHouseNumberWasReaddressed.AddressPersistentLocalId}");
                    houseNumberItem.VersionTimestamp.Should().Be(addressHouseNumberWasReaddressed.Provenance.Timestamp);


                    var boxNumberItem = await ct.AddressVersions.FindAsync(position + 1, addressBoxNumberWasProposedV2.AddressPersistentLocalId);
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
                    boxNumberItem.Type.Should().Be("EventName");
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
                { AddEventHashPipe.HashMetadataKey, addressWasProposedBecauseOfReaddress.GetHash() },
                { Envelope.PositionMetadataKey, position},
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(addressWasProposedBecauseOfReaddress, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position, addressWasProposedBecauseOfReaddress.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasProposedBecauseOfReaddress.StreetNamePersistentLocalId);
                    expectedVersion.ParentPersistentLocalId.Should().Be(addressWasProposedBecauseOfReaddress.ParentPersistentLocalId);
                    expectedVersion.HouseNumber.Should().Be(addressWasProposedBecauseOfReaddress.HouseNumber);
                    expectedVersion.BoxNumber.Should().Be(addressWasProposedBecauseOfReaddress.BoxNumber);
                    expectedVersion.PostalCode.Should().Be(addressWasProposedBecauseOfReaddress.PostalCode);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.PositionMethod.Should().Be(addressWasProposedBecauseOfReaddress.GeometryMethod);
                    expectedVersion.OsloPositionMethod.Should().Be(addressWasProposedBecauseOfReaddress.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should()
                        .Be(addressWasProposedBecauseOfReaddress.GeometrySpecification);
                    expectedVersion.OsloPositionSpecification.Should()
                        .Be(addressWasProposedBecauseOfReaddress.GeometrySpecification.ToPositieSpecificatie());
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasProposedBecauseOfReaddress.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasProposedBecauseOfReaddress.Provenance.Timestamp);

                    var geometry = WKBReaderFactory.CreateForLegacy().Read(addressWasProposedBecauseOfReaddress.ExtendedWkbGeometry.ToByteArray());
                    expectedVersion.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfReaddress()
        {
            var addressWasRejectedBecauseOfReaddress = _fixture.Create<AddressWasRejectedBecauseOfReaddress>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRejectedBecauseOfReaddress.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRejectedBecauseOfReaddress>(new Envelope(addressWasRejectedBecauseOfReaddress, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRejectedBecauseOfReaddress.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasRejectedBecauseOfReaddress.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Rejected);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Rejected.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRejectedBecauseOfReaddress.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRejectedBecauseOfReaddress.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfReaddress()
        {
            var addressWasRetiredBecauseOfReaddress = _fixture.Create<AddressWasRetiredBecauseOfReaddress>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRetiredBecauseOfReaddress.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRetiredBecauseOfReaddress>(new Envelope(addressWasRetiredBecauseOfReaddress, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRetiredBecauseOfReaddress.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasRetiredBecauseOfReaddress.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Removed.Should().BeFalse();
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRetiredBecauseOfReaddress.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRetiredBecauseOfReaddress.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedV2()
        {
            var addressWasRemovedV2 = _fixture.Create<AddressWasRemovedV2>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRemovedV2.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedV2>(new Envelope(addressWasRemovedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRemovedV2.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(addressWasRemovedV2.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeTrue();

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRemovedV2.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRemovedV2.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var addressWasRemovedBecauseStreetNameWasRemoved = _fixture.Create<AddressWasRemovedBecauseStreetNameWasRemoved>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>(new Envelope(addressWasRemovedBecauseStreetNameWasRemoved, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRemovedBecauseStreetNameWasRemoved.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeTrue();

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRemovedBecauseStreetNameWasRemoved.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressWasRemovedBecauseHouseNumberWasRemoved = _fixture.Create<AddressWasRemovedBecauseHouseNumberWasRemoved>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasRemovedBecauseHouseNumberWasRemoved.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(addressWasRemovedBecauseHouseNumberWasRemoved,
                        metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasRemovedBecauseHouseNumberWasRemoved.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasRemovedBecauseHouseNumberWasRemoved.StreetNamePersistentLocalId);
                    expectedVersion.Removed.Should().BeTrue();

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasRemovedBecauseHouseNumberWasRemoved.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRemovedBecauseHouseNumberWasRemoved.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }


        [Fact]
        public async Task WhenAddressWasCorrectedFromRejectedToProposed()
        {
            var addressWasCorrectedFromRejectedToProposed = _fixture.Create<AddressWasCorrectedFromRejectedToProposed>();

            var position = _fixture.Create<long>();

            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressWasCorrectedFromRejectedToProposed.AddressPersistentLocalId);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressWasCorrectedFromRejectedToProposed>(new Envelope(addressWasCorrectedFromRejectedToProposed, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressWasCorrectedFromRejectedToProposed.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressWasCorrectedFromRejectedToProposed.StreetNamePersistentLocalId);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressWasCorrectedFromRejectedToProposed.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasCorrectedFromRejectedToProposed.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressRegularizationWasCorrected()
        {
            var addressRegularizationWasCorrected = _fixture.Create<AddressRegularizationWasCorrected>();

            var position = _fixture.Create<long>();

            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressRegularizationWasCorrected.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Proposed)
                .WithOfficiallyAssigned(true);

            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, proposedMetadata)),
                    new Envelope<AddressRegularizationWasCorrected>(new Envelope(addressRegularizationWasCorrected, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressRegularizationWasCorrected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressRegularizationWasCorrected.StreetNamePersistentLocalId);
                    expectedVersion.OfficiallyAssigned.Should().BeFalse();
                    expectedVersion.Status.Should().Be(AddressStatus.Current);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Current.Map());

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressRegularizationWasCorrected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressRegularizationWasCorrected.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressDeregulationWasCorrected()
        {
            var addressDeregulationWasCorrected = _fixture.Create<AddressDeregulationWasCorrected>();

            var position = _fixture.Create<long>();

            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressDeregulationWasCorrected.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Proposed)
                .WithOfficiallyAssigned(false);
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, proposedMetadata)),
                    new Envelope<AddressDeregulationWasCorrected>(new Envelope(addressDeregulationWasCorrected, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FindAsync(position + 1, addressDeregulationWasCorrected.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should()
                        .Be(addressDeregulationWasCorrected.StreetNamePersistentLocalId);
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressDeregulationWasCorrected.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressDeregulationWasCorrected.Provenance.Timestamp);

                    expectedVersion.OsloPositionMethod.Should().NotBeNullOrEmpty();
                    expectedVersion.OsloPositionSpecification.Should().NotBeNullOrEmpty();
                });
        }

        [Fact]
        public async Task WhenAddressRemovalWasCorrected()
        {
            var @event = _fixture.Create<AddressRemovalWasCorrected>()
                .WithStatus(AddressStatus.Current);

            var position = _fixture.Create<long>();

            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(@event.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Proposed);

            var addressWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "AddressRemovalWasCorrected"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, addressWasMigratedMetadata)),
                    new Envelope<AddressRemovalWasCorrected>(new Envelope(@event, eventMetadata)))
                .Then(async ct =>
                {
                    var expectedGeometry = WKBReaderFactory.CreateForLegacy().Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray());

                    var expectedVersion = await ct.AddressVersions.FindAsync(position, @event.AddressPersistentLocalId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion!.StreetNamePersistentLocalId.Should().Be(@event.StreetNamePersistentLocalId);

                    expectedVersion.ParentPersistentLocalId.Should().Be(@event.ParentPersistentLocalId);
                    expectedVersion.Status.Should().Be(@event.Status);
                    expectedVersion.OsloStatus.Should().Be(@event.Status.Map());
                    expectedVersion.PostalCode.Should().Be(@event.PostalCode);
                    expectedVersion.HouseNumber.Should().Be(@event.HouseNumber);
                    expectedVersion.BoxNumber.Should().Be(@event.BoxNumber);
                    expectedVersion.Geometry.Should().Be(expectedGeometry);
                    expectedVersion.PositionMethod.Should().Be(@event.GeometryMethod);
                    expectedVersion.OsloPositionMethod.Should().Be(@event.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should().Be(@event.GeometrySpecification);
                    expectedVersion.OsloPositionSpecification.Should().Be(@event.GeometrySpecification.ToPositieSpecificatie());
                    expectedVersion.OfficiallyAssigned.Should().Be(@event.OfficiallyAssigned);
                    expectedVersion.Removed.Should().BeFalse();

                    expectedVersion.Type.Should().Be("AddressRemovalWasCorrected");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{@event.AddressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                });
        }

        #region Legacy

        [Fact]
        public async Task WhenAddressWasRegistered()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, metadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position);
                    expectedVersion.PersistentLocalId.Should().Be(addressPersistentLocalId);
                    expectedVersion.StreetNameId.Should().Be(addressWasRegistered.StreetNameId);
                    expectedVersion.HouseNumber.Should().Be(addressWasRegistered.HouseNumber);

                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Namespace.Should().Be(Namespace);
                    expectedVersion.PuriId.Should().Be($"{Namespace}/{addressPersistentLocalId}");
                    expectedVersion.VersionTimestamp.Should().Be(addressWasRegistered.Provenance.Timestamp);
                    expectedVersion.CreatedOnTimestamp.Should().Be(addressWasRegistered.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenAddressBecameComplete()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBecameComplete = _fixture.Create<AddressBecameComplete>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var addressBecameCompleteMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, metadata)),
                    new Envelope<AddressBecameComplete>(new Envelope(addressBecameComplete, addressBecameCompleteMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Position.Should().Be(position + 1);
                });
        }

        [Fact]
        public async Task WhenAddressBecameCurrent()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBecameCurrent = _fixture.Create<AddressBecameCurrent>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressBecameCurrent>(new Envelope(addressBecameCurrent, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().Be(AddressStatus.Current);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Current.Map());
                });
        }

        [Fact]
        public async Task WhenAddressBecameIncomplete()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBecameIncomplete = _fixture.Create<AddressBecameIncomplete>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressBecameIncomplete>(new Envelope(addressBecameIncomplete, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.Position.Should().Be(position + 1);
                });
        }

        [Fact]
        public async Task WhenAddressBecameNotOfficiallyAssigned()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBecameNotOfficiallyAssigned = _fixture.Create<AddressBecameNotOfficiallyAssigned>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressBecameNotOfficiallyAssigned>(new Envelope(addressBecameNotOfficiallyAssigned, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.OfficiallyAssigned.Should().BeFalse();
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasChanged()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressHouseNumberWasChanged = _fixture.Create<AddressHouseNumberWasChanged>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressHouseNumberWasChanged>(new Envelope(addressHouseNumberWasChanged, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.HouseNumber.Should().Be(addressHouseNumberWasChanged.HouseNumber);
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrected()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressHouseNumberWasCorrected = _fixture.Create<AddressHouseNumberWasCorrected>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressHouseNumberWasCorrected>(new Envelope(addressHouseNumberWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.HouseNumber.Should().Be(addressHouseNumberWasCorrected.HouseNumber);
                });
        }

        [Fact]
        public async Task WhenAddressOfficialAssignmentWasRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressOfficialAssignmentWasRemoved = _fixture.Create<AddressOfficialAssignmentWasRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressOfficialAssignmentWasRemoved>(new Envelope(addressOfficialAssignmentWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.OfficiallyAssigned.Should().BeFalse();
                });
        }

        [Fact]
        public async Task WhenAddressPersistentLocalIdWasAssigned()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPersistentLocalIdWasAssigned = _fixture.Create<AddressPersistentLocalIdWasAssigned>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressPersistentLocalIdWasAssigned>(new Envelope(addressPersistentLocalIdWasAssigned, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Type.Should().Be("EventName");
                    expectedVersion.PersistentLocalId.Should().Be(addressPersistentLocalIdWasAssigned.PersistentLocalId);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrected()
        {
            var extendedWkbGeometry = GeometryHelpers.CreateEwkbFrom(
                GeometryHelpers.CreateFromWkt($"POINT ({_fixture.Create<uint>()} {_fixture.Create<uint>()})"));
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPositionWasCorrected = _fixture.Create<AddressPositionWasCorrected>()
                .WithExtendedWkbGeometry(extendedWkbGeometry)
                .WithAddressId(addressWasRegistered.AddressId);

            var position = _fixture.Create<long>();
            var geometry = WKBReaderFactory.CreateForLegacy().Read(addressPositionWasCorrected.ExtendedWkbGeometry.ToByteArray());

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressPositionWasCorrected>(new Envelope(addressPositionWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.PositionMethod.Should().Be(addressPositionWasCorrected.GeometryMethod.ToGeometryMethod());
                    expectedVersion.OsloPositionMethod.Should().Be(addressPositionWasCorrected.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should().Be(addressPositionWasCorrected.GeometrySpecification.ToGeometrySpecification());
                    expectedVersion.OsloPositionSpecification.Should().Be(addressPositionWasCorrected.GeometrySpecification.ToPositieSpecificatie());
                    expectedVersion.Geometry.Should().Be(geometry);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPositionWasRemoved = _fixture.Create<AddressPositionWasRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressPositionWasRemoved>(new Envelope(addressPositionWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.PositionMethod.Should().BeNull();
                    expectedVersion.OsloPositionMethod.Should().BeNull();
                    expectedVersion.PositionSpecification.Should().BeNull();
                    expectedVersion.OsloPositionSpecification.Should().BeNull();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasChanged()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPostalCodeWasChanged = _fixture.Create<AddressPostalCodeWasChanged>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressPostalCodeWasChanged>(new Envelope(addressPostalCodeWasChanged, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.PostalCode.Should().Be(addressPostalCodeWasChanged.PostalCode);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrected()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPostalCodeWasCorrected = _fixture.Create<AddressPostalCodeWasCorrected>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressPostalCodeWasCorrected>(new Envelope(addressPostalCodeWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.PostalCode.Should().Be(addressPostalCodeWasCorrected.PostalCode);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPostalCodeWasRemoved = _fixture.Create<AddressPostalCodeWasRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressPostalCodeWasRemoved>(new Envelope(addressPostalCodeWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.PostalCode.Should().BeNull();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WheAddressStatusWasCorrectedToRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressStatusWasCorrectedToRemoved = _fixture.Create<AddressStatusWasCorrectedToRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressStatusWasCorrectedToRemoved>(new Envelope(addressStatusWasCorrectedToRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().BeNull();
                    expectedVersion.OsloStatus.Should().BeNull();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressStatusWasRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressStatusWasRemoved = _fixture.Create<AddressStatusWasRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressStatusWasRemoved>(new Envelope(addressStatusWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().BeNull();
                    expectedVersion.OsloStatus.Should().BeNull();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressStreetNameWasChanged()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressStreetNameWasChanged = _fixture.Create<AddressStreetNameWasChanged>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressStreetNameWasChanged>(new Envelope(addressStreetNameWasChanged, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.StreetNameId.Should().Be(addressStreetNameWasChanged.StreetNameId);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressStreetNameWasCorrected()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressStreetNameWasCorrected = _fixture.Create<AddressStreetNameWasCorrected>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressStreetNameWasCorrected>(new Envelope(addressStreetNameWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.StreetNameId.Should().Be(addressStreetNameWasCorrected.StreetNameId);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedToCurrent()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasCorrectedToCurrent = _fixture.Create<AddressWasCorrectedToCurrent>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasCorrectedToCurrent>(new Envelope(addressWasCorrectedToCurrent, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().Be(AddressStatus.Current);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Current.Map());
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedToNotOfficiallyAssigned()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasCorrectedToNotOfficiallyAssigned = _fixture.Create<AddressWasCorrectedToNotOfficiallyAssigned>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasCorrectedToNotOfficiallyAssigned>(new Envelope(addressWasCorrectedToNotOfficiallyAssigned, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.OfficiallyAssigned.Should().BeFalse();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedToOfficiallyAssigned()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasCorrectedToOfficiallyAssigned = _fixture.Create<AddressWasCorrectedToOfficiallyAssigned>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasCorrectedToOfficiallyAssigned>(new Envelope(addressWasCorrectedToOfficiallyAssigned, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedToProposed()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasCorrectedToProposed = _fixture.Create<AddressWasCorrectedToProposed>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasCorrectedToProposed>(new Envelope(addressWasCorrectedToProposed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedToRetired()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasCorrectedToRetired = _fixture.Create<AddressWasCorrectedToRetired>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasCorrectedToRetired>(new Envelope(addressWasCorrectedToRetired, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasOfficiallyAssigned()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasOfficiallyAssigned = _fixture.Create<AddressWasOfficiallyAssigned>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasOfficiallyAssigned>(new Envelope(addressWasOfficiallyAssigned, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.OfficiallyAssigned.Should().BeTrue();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }


        [Fact]
        public async Task WhenAddressWasPositioned()
        {
            var extendedWkbGeometry = GeometryHelpers.CreateEwkbFrom(
                GeometryHelpers.CreateFromWkt($"POINT ({_fixture.Create<uint>()} {_fixture.Create<uint>()})"));
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressPositionWasCorrected = _fixture.Create<AddressWasPositioned>()
                .WithExtendedWkbGeometry(extendedWkbGeometry)
                .WithAddressId(addressWasRegistered.AddressId);

            var position = _fixture.Create<long>();
            var geometry = WKBReaderFactory.CreateForLegacy().Read(addressPositionWasCorrected.ExtendedWkbGeometry.ToByteArray());

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasPositioned>(new Envelope(addressPositionWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.PositionMethod.Should().Be(addressPositionWasCorrected.GeometryMethod.ToGeometryMethod());
                    expectedVersion.OsloPositionMethod.Should().Be(addressPositionWasCorrected.GeometryMethod.ToPositieGeometrieMethode());
                    expectedVersion.PositionSpecification.Should().Be(addressPositionWasCorrected.GeometrySpecification.ToGeometrySpecification());
                    expectedVersion.OsloPositionSpecification.Should().Be(addressPositionWasCorrected.GeometrySpecification.ToPositieSpecificatie());
                    expectedVersion.Geometry.Should().Be(geometry);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasProposed()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasProposed = _fixture.Create<AddressWasProposed>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasProposed>(new Envelope(addressWasProposed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().Be(AddressStatus.Proposed);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Proposed.Map());
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasRemoved = _fixture.Create<AddressWasRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasRemoved>(new Envelope(addressWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Removed.Should().BeTrue();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressWasRetired()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressWasRetired = _fixture.Create<AddressWasRetired>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressWasRetired>(new Envelope(addressWasRetired, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.Status.Should().Be(AddressStatus.Retired);
                    expectedVersion.OsloStatus.Should().Be(AddressStatus.Retired.Map());
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasChanged()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBoxNumberWasChanged = _fixture.Create<AddressBoxNumberWasChanged>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressBoxNumberWasChanged>(new Envelope(addressBoxNumberWasChanged, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.BoxNumber.Should().Be(addressBoxNumberWasChanged.BoxNumber);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrected()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBoxNumberWasCorrected = _fixture.Create<AddressBoxNumberWasCorrected>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressBoxNumberWasCorrected>(new Envelope(addressBoxNumberWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.BoxNumber.Should().Be(addressBoxNumberWasCorrected.BoxNumber);
                    expectedVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasRemoved()
        {
            var addressWasRegistered = _fixture.Create<AddressWasRegistered>();
            var addressBoxNumberWasRemoved = _fixture.Create<AddressBoxNumberWasRemoved>();

            var position = _fixture.Create<long>();

            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetAddressPersistentLocalId(addressWasRegistered.AddressId))
                .ReturnsAsync(addressPersistentLocalId);
            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<AddressWasRegistered>(new Envelope(addressWasRegistered, firstEventMetadata)),
                    new Envelope<AddressBoxNumberWasRemoved>(new Envelope(addressBoxNumberWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedVersion =
                        await ct.AddressVersions.FirstAsync(x => x.AddressId == addressWasRegistered.AddressId & x.Position == position +1);
                    expectedVersion.Should().NotBeNull();
                    expectedVersion.Position.Should().Be(position + 1);
                    expectedVersion.BoxNumber.Should().BeNull();
                    expectedVersion.Type.Should().Be("EventName");
                });
        }
        #endregion

        protected override AddressVersionProjections CreateProjection()
            => new AddressVersionProjections(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }), _eventsRepositoryMock.Object);
    }
}
