namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingRejection
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using ProjectionTests.Legacy.Extensions;
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
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasCorrectedToProposedFromRejected()
        {
            var houseNumber = Fixture.Create<HouseNumber>();

            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var addressWasRejected = new AddressWasRejected(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasRejected).SetProvenance(Fixture.Create<Provenance>());

            var migrateRemovedAddressToTestFiltering = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                new AddressPersistentLocalId(456),
                AddressStatus.Current,
                houseNumber,
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedAddressToTestFiltering).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRejected,
                    migrateRemovedAddressToTestFiltering)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressWasCorrectedFromRejectedToProposed(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>()))));
        }

        [Fact]
        public void WhenAddressIsRemoved_ThrowAddressRemovedException()
        {
            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var addressWasRemovedV2 = new AddressWasRemovedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasRemovedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRemovedV2)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WhenAddressIsProposed_ThenDoNothing()
        {
            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WhenParentHouseNumberIsInconsistent_ThenThrowsBoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        {
            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(456),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(123),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber("403"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var addressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                command.AddressPersistentLocalId,
                AddressStatus.Rejected,
                new HouseNumber("404"),
                new BoxNumber("1XYZ"),
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: new AddressPersistentLocalId(123));
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasProposedV2,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()));
        }


        [Fact]
        public void WhenParentPostalCodeIsInconsistent_ThenThrowsBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        {
            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(456),
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(123),
                parentPersistentLocalId: null,
                new PostalCode("9000"),
                new HouseNumber("404"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var addressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                command.AddressPersistentLocalId,
                AddressStatus.Rejected,
                new HouseNumber("404"),
                new BoxNumber("1XYZ"),
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: new PostalCode("1500"),
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: new AddressPersistentLocalId(123));
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasProposedV2,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void OnStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                streetNameStatus);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>()))
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Retired)]
        [InlineData(AddressStatus.Unknown)]
        public void WhenAddressHasInvalidStatus_ThrowAddressRejectionCannotBeCorrectedException(AddressStatus status)
        {
            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var migrateRemovedAddressToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                status,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedAddressToStreetName).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migrateRemovedAddressToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("1A", "1A")]
        [InlineData("1A", "1a")]
        public void WhenAddressAlreadyExists_ThrowAddressAlreadyExistsException(string firstHouseNumber, string secondHouseNumber)
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var address1WasProposed = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber(firstHouseNumber),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)address1WasProposed).SetProvenance(Fixture.Create<Provenance>());

            var address1WasRejected = new AddressWasRejected(
                streetNamePersistentLocalId,
                addressPersistentLocalId);
            ((ISetProvenance)address1WasRejected).SetProvenance(Fixture.Create<Provenance>());

            var address2WasProposed = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(123),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber(secondHouseNumber),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)address2WasProposed).SetProvenance(Fixture.Create<Provenance>());

            var correctAddress1Rejection = new CorrectAddressRejection(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    address1WasProposed,
                    address1WasRejected,
                    address2WasProposed
                    )
                .When(correctAddress1Rejection)
                .Throws(new AddressAlreadyExistsException(new HouseNumber(firstHouseNumber), null)));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WhenParentAddressHasInvalidStatus_ThrowParentAddressHasInvalidStatusException(AddressStatus invalidStatus)
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var parentAddress = new AddressPersistentLocalId(123);
            var childAddress = new AddressPersistentLocalId(456);
            var houseNumber = new HouseNumber("11");

            var migrateParentAddress = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                parentAddress,
                invalidStatus,
                houseNumber,
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: false,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateParentAddress).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasProposed = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                childAddress,
                parentPersistentLocalId: parentAddress,
                Fixture.Create<PostalCode>(),
                houseNumber,
                boxNumber: new BoxNumber("1A"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)childAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var correctChildAddressRejection = new CorrectAddressRejection(
                streetNamePersistentLocalId,
                childAddress,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migrateParentAddress,
                    childAddressWasProposed
                    )
                .When(correctChildAddressRejection)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }

        [Fact]
        public void StateCheck()
        {
            var addressWasCorrectedFromRejectedToProposed = Fixture.Create<AddressWasCorrectedFromRejectedToProposed>();

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasProposedV2>(),
                Fixture.Create<AddressWasProposedV2>().WithParentAddressPersistentLocalId(null),
                Fixture.Create<AddressWasRejected>(),
                addressWasCorrectedFromRejectedToProposed
            });

            var address = sut.StreetNameAddresses.Single(x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.Status.Should().Be(AddressStatus.Proposed);
            address.LastEventHash.Should().Be(addressWasCorrectedFromRejectedToProposed.GetHash());
        }
    }
}
