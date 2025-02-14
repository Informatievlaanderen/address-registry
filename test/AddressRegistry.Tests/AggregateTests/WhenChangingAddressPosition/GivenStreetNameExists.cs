namespace AddressRegistry.Tests.AggregateTests.WhenChangingAddressPosition
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());

            Fixture.Customize<ChangeAddressPosition>(composer =>
            {
                return composer.FromFactory(() => new ChangeAddressPosition(
                    Fixture.Create<StreetNamePersistentLocalId>(),
                    Fixture.Create<AddressPersistentLocalId>(),
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    Fixture.Create<ExtendedWkbGeometry>(),
                    Fixture.Create<Provenance>()));
            });

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressPositionWasChanged()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var command = Fixture.Create<ChangeAddressPosition>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPositionWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        command.Position))));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<ChangeAddressPosition>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var migrateRemovedAddressToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed)
                .WithRemoved();

            var command = Fixture.Create<ChangeAddressPosition>();

            Assert(new Scenario()
                .Given(_streamId,
                    migrateRemovedAddressToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus);

            var command = Fixture.Create<ChangeAddressPosition>();

            Assert(new Scenario()
                .Given(_streamId, addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException(command.AddressPersistentLocalId)));
        }

        [Fact]
        public void WithStreetNameRetired_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var streetNameWasRetired = Fixture.Create<StreetNameWasRetired>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command = Fixture.Create<ChangeAddressPosition>();

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName,
                    streetNameWasRetired)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithStreetNameRejected_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var streetNameWasRejected = Fixture.Create<StreetNameWasRejected>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command = Fixture.Create<ChangeAddressPosition>();

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName,
                    streetNameWasRejected)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithInvalidGeometryMethod_ThenThrowsAddressHasInvalidGeometryMethodException()
        {
            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                GeometryMethod.Interpolated,
                GeometrySpecification.Entry,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Throws(new AddressHasInvalidGeometryMethodException()));
        }

        [Theory]
        [InlineData(GeometrySpecification.RoadSegment)]
        [InlineData(GeometrySpecification.Municipality)]
        [InlineData(GeometrySpecification.Building)]
        [InlineData(GeometrySpecification.Street)]
        public void WithGeometryMethodAppointedByAdministratorAndInvalidSpecification_ThenThrowsAddressHasInvalidGeometrySpecificationException(
            GeometrySpecification specification)
        {
            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                GeometryMethod.AppointedByAdministrator,
                specification,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Throws(new AddressHasInvalidGeometrySpecificationException()));
        }

        [Theory]
        [InlineData(GeometrySpecification.Municipality)]
        [InlineData(GeometrySpecification.Entry)]
        [InlineData(GeometrySpecification.Lot)]
        [InlineData(GeometrySpecification.Stand)]
        [InlineData(GeometrySpecification.Berth)]
        public void WithGeometryMethodDerivedFromObjectAndInvalidSpecification_ThenThrowsAddressHasInvalidGeometrySpecificationException(
            GeometrySpecification specification)
        {
            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                GeometryMethod.DerivedFromObject,
                specification,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Throws(new AddressHasInvalidGeometrySpecificationException()));
        }

        [Fact]
        public void WithNoChangedPosition_ThenNone()
        {
            var geometryMethod = GeometryMethod.AppointedByAdministrator;
            var geometrySpecification = GeometrySpecification.Entry;
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current)
                .WithAddressGeometry(new AddressGeometry(geometryMethod, geometrySpecification, extendedWkbGeometry));

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                geometryMethod,
                geometrySpecification,
                extendedWkbGeometry,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var geometryMethod = GeometryMethod.AppointedByAdministrator;
            var geometrySpecification = GeometrySpecification.Entry;
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry())
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry);

            var addressPositionWasChanged = new AddressPositionWasChanged(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                geometryMethod,
                geometrySpecification,
                extendedWkbGeometry);
            ((ISetProvenance)addressPositionWasChanged).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { addressWasProposedV2, addressPositionWasChanged });

            // Act
            sut.ChangeAddressPosition(
                Fixture.Create<AddressPersistentLocalId>(),
                geometryMethod,
                geometrySpecification,
                extendedWkbGeometry);

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            parentAddress.Geometry.Should().Be(new AddressGeometry(geometryMethod, geometrySpecification, extendedWkbGeometry));
        }
    }
}
