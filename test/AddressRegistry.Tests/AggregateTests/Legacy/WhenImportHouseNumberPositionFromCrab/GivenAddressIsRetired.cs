namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRetired : AddressRegistryTest
    {
        public GivenAddressIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void WhenLifetimeHasNoEndDate(
            IFixture fixture,
            AddressId addressId,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab.WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding);

            Assert(RetiredAddressScenario(fixture)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultData]
        public void WhenLifetimeHasEndDate(
            IFixture fixture,
            AddressId addressId,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithLifetime(new CrabLifetime(importHouseNumberPositionFromCrab.Lifetime.BeginDateTime.Value, importHouseNumberPositionFromCrab.Lifetime.BeginDateTime.Value.PlusDays(1)));

            Assert(RetiredAddressScenario(fixture)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }
    }
}
