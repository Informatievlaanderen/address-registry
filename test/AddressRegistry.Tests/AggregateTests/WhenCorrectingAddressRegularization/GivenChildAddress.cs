namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressRegularization
{
    using StreetName;
    using StreetName.Commands;
    using AutoFixture;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenChildAddress : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenChildAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WhenParentAddressIsProposed()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(456);

            var command = new CorrectAddressRegularization(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposed = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                parentPersistentLocalId: null,
                new PostalCode("2018"),
                new HouseNumber("11"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasProposed = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId,
                parentPersistentLocalId: parentAddressPersistentLocalId,
                new PostalCode("2018"),
                new HouseNumber("11"),
                new BoxNumber("001"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)childAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasRegularized = new AddressWasRegularized(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId);
            ((ISetProvenance)childAddressWasRegularized).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposed,
                    childAddressWasProposed)
                .When(command)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WhenParentAddressIsNotProposed(AddressStatus status)
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(456);

            var command = new CorrectAddressRegularization(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var migrateParentAddress = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                parentAddressPersistentLocalId,
                status,
                new HouseNumber("11"),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: new PostalCode("2018"),
                isCompleted: true,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateParentAddress).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasProposed = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId,
                parentPersistentLocalId: parentAddressPersistentLocalId,
                new PostalCode("2018"),
                new HouseNumber("11"),
                new BoxNumber("001"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)childAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasRegularized = new AddressWasRegularized(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId);
            ((ISetProvenance)childAddressWasRegularized).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migrateParentAddress,
                    childAddressWasProposed)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressRegularizationWasCorrected(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        childAddressPersistentLocalId))));
        }
    }
}
