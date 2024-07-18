namespace AddressRegistry.Tests.AggregateTests.WhenProposingAddressForMunicipalityMerger
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedValidHouseNumber());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithExistingParent_ThenAddressWasProposed()
        {
            var houseNumber = Fixture.Create<HouseNumber>();
            var postalCode = Fixture.Create<PostalCode>();

            var parentAddressWasProposed = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                postalCode,
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                officiallyAssigned: Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                postalCode,
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                new BoxNumber("1A"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            var proposeChildAddresses = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [proposeChildAddress],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed)
                .When(proposeChildAddresses)
                .Then(
                    new Fact(_streamId,
                        new AddressWasProposedForMunicipalityMerger(
                            proposeChildAddresses.StreetNamePersistentLocalId,
                            proposeChildAddress.AddressPersistentLocalId,
                            new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                            proposeChildAddress.PostalCode,
                            proposeChildAddress.HouseNumber,
                            proposeChildAddress.BoxNumber,
                            GeometryMethod.AppointedByAdministrator,
                            GeometrySpecification.Entry,
                            GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                            proposeChildAddress.OfficiallyAssigned,
                            proposeChildAddress.MergedAddressPersistentLocalId))));
        }

        [Fact]
        public void WithExistingParentRemoved_ThenParentAddressNotFoundExceptionWasThrown()
        {
            var houseNumber = Fixture.Create<HouseNumber>();

            var parentAddressWasProposed = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var parentAddressWasRemoved = new AddressWasRemovedV2(
                new StreetNamePersistentLocalId(parentAddressWasProposed.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId));

            ((ISetProvenance)parentAddressWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                Fixture.Create<BoxNumber>(),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            var proposeChildAddresses = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [proposeChildAddress],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed,
                    parentAddressWasRemoved)
                .When(proposeChildAddresses)
                .Throws(new ParentAddressNotFoundException(
                    new StreetNamePersistentLocalId(parentAddressWasProposed.StreetNamePersistentLocalId),
                    houseNumber)));
        }

        [Fact]
        public void ChildAddressWithoutExistingParent_ThenThrowsParentNotFoundException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var houseNumber = Fixture.Create<HouseNumber>();

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                Fixture.Create<BoxNumber>(),
                GeometryMethod.DerivedFromObject,
                GeometrySpecification.Municipality,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            var proposeChildAddresses = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeChildAddress],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(proposeChildAddresses)
                .Throws(new ParentAddressNotFoundException(streetNamePersistentLocalId, houseNumber)));
        }

        [Fact]
        public void ParentAddress_ThenAddressWasProposed()
        {
            var proposeParentAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("1"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            var proposeParentAddresses = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [proposeParentAddress],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(proposeParentAddresses)
                .Then(
                    new Fact(_streamId,
                        new AddressWasProposedForMunicipalityMerger(
                            proposeParentAddresses.StreetNamePersistentLocalId,
                            proposeParentAddress.AddressPersistentLocalId,
                            parentPersistentLocalId: null,
                            proposeParentAddress.PostalCode,
                            proposeParentAddress.HouseNumber,
                            boxNumber: null,
                            GeometryMethod.AppointedByAdministrator,
                            GeometrySpecification.Entry,
                            GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                            proposeParentAddress.OfficiallyAssigned,
                            proposeParentAddress.MergedAddressPersistentLocalId))));
        }

        [Fact]
        public void WithExistingPersistentLocalId_ThenThrowsAddressPersistentLocalIdAlreadyExistsException()
        {
            var addressWasProposedForMunicipalityMerger = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            var proposeAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                new AddressPersistentLocalId(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId),
                new HouseNumber(Fixture.Create<string>()),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [proposeAddress],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    addressWasProposedForMunicipalityMerger)
                .When(command)
                .Throws(new AddressPersistentLocalIdAlreadyExistsException()));
        }

        [Theory]
        [InlineData("1A", "1A")]
        [InlineData("1A", "1a")]
        public void WithHouseNumberAlreadyInUse_ThenThrowsParentAddressAlreadyExistsException(string existingHouseNumber, string houseNumberToPropose)
        {
            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    Fixture.Create<PostalCode>(),
                    new AddressPersistentLocalId(200),
                    new HouseNumber(houseNumberToPropose),
                    boxNumber: null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            var addressWasProposedForMunicipalityMerger = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(100),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber(existingHouseNumber),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedForMunicipalityMerger)
                .When(command)
                .Throws(new ParentAddressAlreadyExistsException(new HouseNumber(houseNumberToPropose))));
        }

        [Theory]
        [InlineData("A", "A")]
        [InlineData("A", "a")]
        public void WithHouseNumberAndBoxNumberAlreadyInUse_ThenThrowsAddressAlreadyExistsException(string existingBoxNumber, string boxNumberToPropose)
        {
            var postalCode = Fixture.Create<PostalCode>();

            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    postalCode,
                    new AddressPersistentLocalId(200),
                    new HouseNumber("1A"),
                    new BoxNumber(boxNumberToPropose),
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            var parentAddressWasProposedForMunicipalityMerger = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(100),
                parentPersistentLocalId: null,
                postalCode,
                new HouseNumber("1a"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)parentAddressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            var addressWasProposedForMunicipalityMerger = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(101),
                parentPersistentLocalId: new AddressPersistentLocalId(100),
                postalCode,
                new HouseNumber("1a"),
                new BoxNumber(existingBoxNumber),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposedForMunicipalityMerger,
                    addressWasProposedForMunicipalityMerger)
                .When(command)
                .Throws(new AddressAlreadyExistsException(new HouseNumber("1A"), new BoxNumber(boxNumberToPropose))));
        }

        [Fact]
        public void WithInvalidGeometryMethod_ThenThrowsAddressHasInvalidGeometryMethodException()
        {
            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    Fixture.Create<PostalCode>(),
                    Fixture.Create<AddressPersistentLocalId>(),
                    new HouseNumber("1"),
                    null,
                    GeometryMethod.Interpolated,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressHasInvalidGeometryMethodException()));
        }

        [Theory]
        [InlineData(GeometrySpecification.RoadSegment)]
        [InlineData(GeometrySpecification.Municipality)]
        [InlineData(GeometrySpecification.Building)]
        [InlineData(GeometrySpecification.Street)]
        public void WithAppointedByAdministratorAndInvalidSpecification_ThenThrowsAddressHasInvalidGeometrySpecificationException(GeometrySpecification invalidSpecification)
        {
            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    Fixture.Create<PostalCode>(),
                    Fixture.Create<AddressPersistentLocalId>(),
                    new HouseNumber("1"),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    invalidSpecification,
                    Fixture.Create<ExtendedWkbGeometry>(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressHasInvalidGeometrySpecificationException()));
        }

        [Theory]
        [InlineData(GeometrySpecification.Municipality)]
        [InlineData(GeometrySpecification.Entry)]
        [InlineData(GeometrySpecification.Lot)]
        [InlineData(GeometrySpecification.Stand)]
        [InlineData(GeometrySpecification.Berth)]
        public void WithGeometryMethodDerivedFromObjectAndInvalidSpecification_ThenThrowsAddressHasInvalidGeometrySpecificationException(GeometrySpecification invalidSpecification)
        {
            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    Fixture.Create<PostalCode>(),
                    Fixture.Create<AddressPersistentLocalId>(),
                    new HouseNumber("1"),
                    null,
                    GeometryMethod.DerivedFromObject,
                    invalidSpecification,
                    Fixture.Create<ExtendedWkbGeometry>(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressHasInvalidGeometrySpecificationException()));
        }

        [Fact]
        public void WithBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCode_ThenThrowsBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        {
            var houseNumber = Fixture.Create<HouseNumber>();

            var parentAddressWasProposed = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                new PostalCode("9000"),
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    Fixture.Create<AddressPersistentLocalId>(),
                    houseNumber,
                    new BoxNumber("1A"),
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed)
                .When(proposeChildAddress)
                .Throws(new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithStreetNameHasInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(), Fixture.Create<NisCode>(),
                streetNameStatus);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    Fixture.Create<AddressPersistentLocalId>(),
                    Fixture.Create<HouseNumber>(),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, migratedStreetNameWasImported)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithRemovedStreetName_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var migratedStreetNameWasImported = new  MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(), Fixture.Create<NisCode>(),
                StreetNameStatus.Current);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasRemoved = new StreetNameWasRemoved(Fixture.Create<StreetNamePersistentLocalId>());
            ((ISetProvenance)streetNameWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    Fixture.Create<AddressPersistentLocalId>(),
                    Fixture.Create<HouseNumber>(),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, migratedStreetNameWasImported, streetNameWasRemoved)
                .When(command)
                .Throws(new StreetNameIsRemovedException(Fixture.Create<StreetNamePersistentLocalId>())));
        }

        [Fact]
        public void WithInvalidMergedAddressPersistentLocalId_ThenThrowsMergedAddressPersistentLocalIdIsInvalidException()
        {
            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(), Fixture.Create<NisCode>(),
                StreetNameStatus.Current);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    addressPersistentLocalId,
                    Fixture.Create<HouseNumber>(),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    addressPersistentLocalId
                )],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, migratedStreetNameWasImported)
                .When(command)
                .Throws(new MergedAddressPersistentLocalIdIsInvalidException()));
        }

        [Fact]
        public void WithFirstChildAndThenParentAddress_ThenAddressesWereProposed()
        {
            var houseNumber = Fixture.Create<HouseNumber>();
            var postalCode = Fixture.Create<PostalCode>();

            var proposeParentAddress = new ProposeAddressesForMunicipalityMergerItem(
                postalCode,
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                proposeParentAddress.PostalCode,
                Fixture.Create<AddressPersistentLocalId>(),
                proposeParentAddress.HouseNumber,
                new BoxNumber("1A"),
                proposeParentAddress.GeometryMethod,
                proposeParentAddress.GeometrySpecification,
                proposeParentAddress.Position,
                proposeParentAddress.OfficiallyAssigned,
                Fixture.Create<AddressPersistentLocalId>());

            var proposeAddresses = new ProposeAddressesForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                [proposeChildAddress, proposeParentAddress],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(proposeAddresses)
                .Then(_streamId,
                    new AddressWasProposedForMunicipalityMerger(
                        proposeAddresses.StreetNamePersistentLocalId,
                        proposeParentAddress.AddressPersistentLocalId,
                        null,
                        proposeParentAddress.PostalCode,
                        proposeParentAddress.HouseNumber,
                        proposeParentAddress.BoxNumber,
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                        proposeParentAddress.OfficiallyAssigned,
                        proposeParentAddress.MergedAddressPersistentLocalId),
                    new AddressWasProposedForMunicipalityMerger(
                        proposeAddresses.StreetNamePersistentLocalId,
                        proposeChildAddress.AddressPersistentLocalId,
                        new AddressPersistentLocalId(proposeParentAddress.AddressPersistentLocalId),
                        proposeChildAddress.PostalCode,
                        proposeChildAddress.HouseNumber,
                        proposeChildAddress.BoxNumber,
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                        proposeChildAddress.OfficiallyAssigned,
                        proposeChildAddress.MergedAddressPersistentLocalId)
                ));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();

            var postalCode = Fixture.Create<PostalCode>();
            var houseNumber = Fixture.Create<HouseNumber>();

            var geometryMethod = GeometryMethod.AppointedByAdministrator;
            var geometrySpecification = GeometrySpecification.Entry;
            var geometryPosition = GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry();

            var childPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var childBoxNumber = Fixture.Create<BoxNumber>();
            var officiallyAssigned = Fixture.Create<bool>();
            var mergedAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var parentAddressWasProposed = new AddressWasProposedForMunicipalityMerger(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                postalCode,
                houseNumber,
                boxNumber: null,
                geometryMethod,
                geometrySpecification,
                geometryPosition,
                officiallyAssigned,
                mergedAddressPersistentLocalId);
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var migrateRemovedIdenticalParentAddress = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                AddressStatus.Current,
                houseNumber,
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedIdenticalParentAddress).SetProvenance(Fixture.Create<Provenance>());

            var migrateRemovedIdenticalChildAddress = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                AddressStatus.Current,
                houseNumber,
                boxNumber: childBoxNumber,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedIdenticalChildAddress).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MigratedStreetNameWasImported>(),
                parentAddressWasProposed,
                migrateRemovedIdenticalParentAddress,
                migrateRemovedIdenticalChildAddress
            });

            // Act
            aggregate.ProposeAddressForMunicipalityMerger(
                childPersistentLocalId,
                postalCode,
                houseNumber,
                childBoxNumber,
                geometryMethod,
                geometrySpecification,
                geometryPosition,
                officiallyAssigned,
                mergedAddressPersistentLocalId);

            // Assert
            var result = aggregate.StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId));
            result.Should().NotBeNull();
            result.Children.Count.Should().Be(1);
            var child = result.Children.Single();
            child.AddressPersistentLocalId.Should().Be(childPersistentLocalId);
            child.HouseNumber.Should().Be(houseNumber);
            child.PostalCode.Should().Be(postalCode);
            child.Status.Should().Be(AddressStatus.Proposed);
            child.BoxNumber.Should().Be(childBoxNumber);
            child.IsOfficiallyAssigned.Should().Be(officiallyAssigned);
            child.Geometry.GeometryMethod.Should().Be(geometryMethod);
            child.Geometry.GeometrySpecification.Should().Be(geometrySpecification);
            child.Geometry.Geometry.Should().Be(geometryPosition);
            child.MergedAddressPersistentLocalId.Should().Be(mergedAddressPersistentLocalId);
        }
    }
}
