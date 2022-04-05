namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenOfficiallyAssignedAddressWithStatus : AddressRegistryTest
    {
        public GivenOfficiallyAssignedAddressWithStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultDataForSubaddress]
        public void ThenAddressBecameComplete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            importSubaddressPositionFromCrab.WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            Assert(new Scenario()
            .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned)
            .When(importSubaddressPositionFromCrab)
            .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    new AddressBecameComplete(addressId),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }
    }
}
