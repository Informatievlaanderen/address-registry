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
    }
}
