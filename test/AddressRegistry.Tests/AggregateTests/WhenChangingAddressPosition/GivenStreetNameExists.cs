namespace AddressRegistry.Tests.AggregateTests.WhenChangingAddressPosition
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using AutoFixture;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly IMunicipalities _municipalities;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _municipalities = Container.Resolve<IMunicipalities>();
        }

        [Fact]
        public void ThenAddressPositionWasChanged()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                extendedWkbGeometry,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPositionWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        addressPersistentLocalId,
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        extendedWkbGeometry))));
        }

        [Fact]
        public void WithGeometryMethodDerivedFromObject_ThenSpecificationIsMunicipalityAndPositionIsMunicipalityCentroid()
        {
            var municipalityId = Fixture.Create<MunicipalityId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.DerivedFromObject,
                null,
                extendedWkbGeometry,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var municipalities = Container.Resolve<TestMunicipalityConsumerContext>();
            var municipalityLatestItem = municipalities.AddMunicipality(municipalityId, GeometryHelpers.ValidGmlPolygon);
            var municipalityCentroid =
                ExtendedWkbGeometry.CreateEWkb(
                    WKBReaderFactory.Create().Read(municipalityLatestItem.ExtendedWkbGeometry).Centroid.Centroid.AsBinary());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPositionWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        addressPersistentLocalId,
                        GeometryMethod.DerivedFromObject,
                        GeometrySpecification.Municipality,
                        municipalityCentroid))));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(addressPersistentLocalId)));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var migrateRemovedAddressToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId,
                AddressStatus.Proposed,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedAddressToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    migrateRemovedAddressToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(addressPersistentLocalId)));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var addressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId,
                addressStatus,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null);
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithGeometryMethodAppointedByAdministratorAndNoSpecification_ThenThrowsAddressHasMissingGeometrySpecificationException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.AppointedByAdministrator,
                null,
                extendedWkbGeometry,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasProposedV2)
                .When(command)
                .Throws(new AddressHasMissingGeometrySpecificationException()));
        }

        [Fact]
        public void WithInvalidGeometryMethod_ThenThrowsAddressHasInvalidGeometryMethodException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.Interpolated,
                GeometrySpecification.Entry,
                extendedWkbGeometry,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasProposedV2)
                .When(command)
                .Throws(new AddressHasInvalidGeometryMethodException()));
        }

        [Theory]
        [InlineData(GeometrySpecification.RoadSegment)]
        [InlineData(GeometrySpecification.Municipality)]
        [InlineData(GeometrySpecification.Building)]
        [InlineData(GeometrySpecification.BuildingUnit)]
        [InlineData(GeometrySpecification.Street)]
        public void WithGeometryMethodAppointedByAdministratorAndInvalidSpecification_ThenThrowsAddressHasInvalidGeometrySpecificationException(GeometrySpecification specification)
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();
            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                GeometryMethod.AppointedByAdministrator,
                specification,
                extendedWkbGeometry,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasProposedV2)
                .When(command)
                .Throws(new AddressHasInvalidGeometrySpecificationException()));
        }

        [Fact]
        public void WithNoChangedPosition_ThenNone()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var geometryMethod = GeometryMethod.AppointedByAdministrator;
            var geometrySpecification = GeometrySpecification.Entry;
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var addressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId,
                AddressStatus.Current,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                new AddressGeometry(geometryMethod, geometrySpecification, extendedWkbGeometry),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null);
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var command = new ChangeAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
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
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var geometryMethod = GeometryMethod.AppointedByAdministrator;
            var geometrySpecification = GeometrySpecification.Entry;
            var extendedWkbGeometry = Fixture.Create<ExtendedWkbGeometry>();

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var addressPositionWasChanged = new AddressPositionWasChanged(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                geometryMethod,
                geometrySpecification,
                extendedWkbGeometry);
            ((ISetProvenance)addressPositionWasChanged).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { addressWasProposedV2, addressPositionWasChanged });

            // Act
            sut.ChangeAddressPosition(
                addressPersistentLocalId,
                geometryMethod,
                geometrySpecification,
                extendedWkbGeometry,
                _municipalities);

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            parentAddress.Geometry.Should().Be(new AddressGeometry(geometryMethod, geometrySpecification, extendedWkbGeometry));
        }
    }
}
