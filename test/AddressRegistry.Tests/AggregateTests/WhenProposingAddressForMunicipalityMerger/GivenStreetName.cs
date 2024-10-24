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
    using EventBuilders;
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
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

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
                Fixture.Create<AddressPersistentLocalId>(),
                AddressStatus.Proposed);
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                postalCode,
                Fixture.Create<AddressPersistentLocalId>(),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(houseNumber),
                new ProposeAddressesForMunicipalityMergerItem.BoxNumberForMunicipalityMerger("1A"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeChildAddress],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasProposedForMunicipalityMerger(
                            command.StreetNamePersistentLocalId,
                            proposeChildAddress.AddressPersistentLocalId,
                            new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                            proposeChildAddress.PostalCode,
                            proposeChildAddress.HouseNumber,
                            proposeChildAddress.BoxNumber,
                            GeometryMethod.AppointedByAdministrator,
                            GeometrySpecification.Entry,
                            GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                            proposeChildAddress.OfficiallyAssigned,
                            proposeChildAddress.MergedAddressPersistentLocalId,
                            parentAddressWasProposed.DesiredStatus))));
        }

        [Fact]
        public void WithExistingParentRemoved_ThenParentAddressNotFoundExceptionWasThrown()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

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
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>());
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var parentAddressWasRemoved = new AddressWasRemovedV2(
                new StreetNamePersistentLocalId(parentAddressWasProposed.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId));
            ((ISetProvenance)parentAddressWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(houseNumber),
                Fixture.Create<ProposeAddressesForMunicipalityMergerItem.BoxNumberForMunicipalityMerger>(),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeChildAddress],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed,
                    parentAddressWasRemoved)
                .When(command)
                .Throws(new ParentAddressNotFoundException(
                    new StreetNamePersistentLocalId(parentAddressWasProposed.StreetNamePersistentLocalId),
                    houseNumber)));
        }

        [Fact]
        public void ChildAddressWithoutExistingParent_ThenThrowsParentAddressNotFoundException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);
            var houseNumber = Fixture.Create<HouseNumber>();

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(houseNumber),
                Fixture.Create<ProposeAddressesForMunicipalityMergerItem.BoxNumberForMunicipalityMerger>(),
                GeometryMethod.DerivedFromObject,
                GeometrySpecification.Municipality,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());
            var proposeChildAddresses = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeChildAddress],
                Fixture.Create<Provenance>());
            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(proposeChildAddress.MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(proposeChildAddress.MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(proposeChildAddresses)
                .Throws(new ParentAddressNotFoundException(streetNamePersistentLocalId, houseNumber)));
        }

        [Fact]
        public void ParentAddress_ThenAddressWasProposed()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var proposeParentAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger("1"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());

            var proposeParentAddresses = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeParentAddress],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(proposeParentAddress.MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(proposeParentAddress.MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
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
                            proposeParentAddress.MergedAddressPersistentLocalId,
                            oldAddressWasMigrated.Status
                        ))));
        }

        [Fact]
        public void WithExistingPersistentLocalId_ThenThrowsAddressPersistentLocalIdAlreadyExistsException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

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
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            var proposeAddress = new ProposeAddressesForMunicipalityMergerItem(
                Fixture.Create<PostalCode>(),
                new AddressPersistentLocalId(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(Fixture.Create<string>()),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeAddress],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
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
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [new ProposeAddressesForMunicipalityMergerItem(
                    Fixture.Create<PostalCode>(),
                    new AddressPersistentLocalId(200),
                    new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(houseNumberToPropose),
                    boxNumber: null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    oldStreetNamePersistentLocalId,
                    new AddressPersistentLocalId(1)
                )],
                Fixture.Create<Provenance>());

            var addressWasProposedForMunicipalityMerger = new AddressWasProposedForMunicipalityMerger(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(100),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber(existingHouseNumber),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                new AddressPersistentLocalId(2),
                Fixture.Create<AddressStatus>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
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
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var postalCode = Fixture.Create<PostalCode>();

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [new ProposeAddressesForMunicipalityMergerItem(
                    postalCode,
                    new AddressPersistentLocalId(200),
                    new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger("1A"),
                    new ProposeAddressesForMunicipalityMergerItem.BoxNumberForMunicipalityMerger(boxNumberToPropose),
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    oldStreetNamePersistentLocalId,
                    new AddressPersistentLocalId(201)
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
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>());
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
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposedForMunicipalityMerger,
                    addressWasProposedForMunicipalityMerger)
                .When(command)
                .Throws(new AddressAlreadyExistsException(new HouseNumber("1A"), new BoxNumber(boxNumberToPropose))));
        }

        [Fact]
        public void WithBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCode_ThenThrowsBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var houseNumber = Fixture.Create<HouseNumber>();

            var parentAddressWasProposed = new AddressWasProposedForMunicipalityMerger(
                streetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                new PostalCode("9000"),
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>());
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    Fixture.Create<AddressPersistentLocalId>(),
                    new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(houseNumber),
                    new ProposeAddressesForMunicipalityMergerItem.BoxNumberForMunicipalityMerger("1A"),
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    oldStreetNamePersistentLocalId,
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed)
                .When(command)
                .Throws(new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithStreetNameHasInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(), Fixture.Create<NisCode>(),
                streetNameStatus);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    Fixture.Create<AddressPersistentLocalId>(),
                    Fixture.Create<ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger>(),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    oldStreetNamePersistentLocalId,
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId, migratedStreetNameWasImported)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithRemovedStreetName_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var migratedStreetNameWasImported = new  MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(), Fixture.Create<NisCode>(),
                StreetNameStatus.Current);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasRemoved = new StreetNameWasRemoved(Fixture.Create<StreetNamePersistentLocalId>());
            ((ISetProvenance)streetNameWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    Fixture.Create<AddressPersistentLocalId>(),
                    Fixture.Create<ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger>(),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    oldStreetNamePersistentLocalId,
                    Fixture.Create<AddressPersistentLocalId>()
                )],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId, migratedStreetNameWasImported, streetNameWasRemoved)
                .When(command)
                .Throws(new StreetNameIsRemovedException(Fixture.Create<StreetNamePersistentLocalId>())));
        }

        [Fact]
        public void WithInvalidMergedAddressPersistentLocalId_ThenThrowsMergedAddressPersistentLocalIdIsInvalidException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(), Fixture.Create<NisCode>(),
                StreetNameStatus.Current);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [new ProposeAddressesForMunicipalityMergerItem(
                    new PostalCode("9820"),
                    addressPersistentLocalId,
                    Fixture.Create<ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger>(),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    Fixture.Create<bool>(),
                    oldStreetNamePersistentLocalId,
                    addressPersistentLocalId
                )],
                Fixture.Create<Provenance>());

            var oldAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(command.Addresses.First().MergedStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(command.Addresses.First().MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldAddressWasMigrated)
                .Given(_streamId, migratedStreetNameWasImported)
                .When(command)
                .Throws(new MergedAddressPersistentLocalIdIsInvalidException()));
        }

        [Fact]
        public void WithFirstChildAndThenParentAddress_ThenAddressesWereProposed()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var oldStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);
            var houseNumber = Fixture.Create<HouseNumber>();
            var postalCode = Fixture.Create<PostalCode>();

            var proposeParentAddress = new ProposeAddressesForMunicipalityMergerItem(
                postalCode,
                Fixture.Create<AddressPersistentLocalId>(),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(houseNumber),
                null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<bool>(),
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());

            var proposeChildAddress = new ProposeAddressesForMunicipalityMergerItem(
                proposeParentAddress.PostalCode,
                Fixture.Create<AddressPersistentLocalId>(),
                new ProposeAddressesForMunicipalityMergerItem.HouseNumberForMunicipalityMerger(proposeParentAddress.HouseNumber),
                new ProposeAddressesForMunicipalityMergerItem.BoxNumberForMunicipalityMerger("1A"),
                proposeParentAddress.GeometryMethod,
                proposeParentAddress.GeometrySpecification,
                proposeParentAddress.Position,
                proposeParentAddress.OfficiallyAssigned,
                oldStreetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>());

            var command = new ProposeAddressesForMunicipalityMerger(
                streetNamePersistentLocalId,
                [proposeChildAddress, proposeParentAddress],
                Fixture.Create<Provenance>());

            var oldParentAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(oldStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(proposeParentAddress.MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();
            var oldChildAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(oldStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(proposeChildAddress.MergedAddressPersistentLocalId)
                .WithStatus(Fixture.Create<bool>() ? AddressStatus.Proposed : AddressStatus.Current)
                .Build();

            Assert(new Scenario()
                .Given(new StreetNameStreamId(oldStreetNamePersistentLocalId),
                    oldParentAddressWasMigrated, oldChildAddressWasMigrated)
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(command)
                .Then(_streamId,
                    new AddressWasProposedForMunicipalityMerger(
                        command.StreetNamePersistentLocalId,
                        proposeParentAddress.AddressPersistentLocalId,
                        null,
                        proposeParentAddress.PostalCode,
                        proposeParentAddress.HouseNumber,
                        proposeParentAddress.BoxNumber,
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                        proposeParentAddress.OfficiallyAssigned,
                        proposeParentAddress.MergedAddressPersistentLocalId,
                        oldParentAddressWasMigrated.Status),
                    new AddressWasProposedForMunicipalityMerger(
                        command.StreetNamePersistentLocalId,
                        proposeChildAddress.AddressPersistentLocalId,
                        new AddressPersistentLocalId(proposeParentAddress.AddressPersistentLocalId),
                        proposeChildAddress.PostalCode,
                        proposeChildAddress.HouseNumber,
                        proposeChildAddress.BoxNumber,
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                        proposeChildAddress.OfficiallyAssigned,
                        proposeChildAddress.MergedAddressPersistentLocalId,
                        oldChildAddressWasMigrated.Status)
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
            var desiredStatus = AddressStatus.Proposed;

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
                mergedAddressPersistentLocalId,
                desiredStatus);
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
                mergedAddressPersistentLocalId,
                desiredStatus);

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
            child.DesiredStatusAfterMunicipalityMerger.Should().Be(desiredStatus);
        }
    }
}
