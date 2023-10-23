namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberFromCrab
{
    using System;
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using EventExtensions;
    using global::AutoFixture;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressWithPosition : AddressRegistryTest
    {
        public GivenAddressWithPosition(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void GivenAddressPositionHasEndDate(
            IFixture fixture,
            AddressId addressId,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImported,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            addressHouseNumberPositionWasImported = addressHouseNumberPositionWasImported
                    .WithEndDate(DateTimeOffset.Now)
                    .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding);

            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), LocalDateTime.FromDateTime(DateTime.Now.AddDays(1))));

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressHouseNumberPositionWasImported)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasRetired(addressId),
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(new WkbGeometry(addressHouseNumberPositionWasImported.AddressPosition)))),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }
    }
}
