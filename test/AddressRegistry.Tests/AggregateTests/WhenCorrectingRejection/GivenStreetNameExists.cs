namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingRejection
{
    using Address.Exceptions;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasCorrectedToProposedFromRejected()
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

            var addressWasRejected = new AddressWasRejected(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasRejected).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRejected)
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

        [Fact]
        public void WhenAddressAlreadyExists_ThrowAddressAlreadyExistsException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var houseNumber = new HouseNumber("11");

            var address1WasProposed = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
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
                houseNumber,
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
                .Throws(new AddressAlreadyExistsException(houseNumber, null)));
        }

        [Theory]
        [InlineData(AddressStatus.Proposed)]
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
    }
}
