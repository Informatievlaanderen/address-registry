namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using Address;
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

        [Theory, DefaultData]
        public void ThenAddressBecameComplete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            importHouseNumberPositionFromCrab.WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            Assert(new Scenario()
            .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned)
            .When(importHouseNumberPositionFromCrab)
            .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    new AddressBecameComplete(addressId),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }
    }
}
