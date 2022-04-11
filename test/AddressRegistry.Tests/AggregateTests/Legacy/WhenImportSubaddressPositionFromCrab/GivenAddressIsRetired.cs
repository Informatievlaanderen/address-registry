namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressPositionFromCrab
{
    using Address;
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

        [Theory, DefaultDataForSubaddress]
        public void WhenLifetimeHasNoEndDate(
            IFixture fixture,
            AddressId addressId,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab.WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding);

            Assert(RetiredAddressScenario(fixture)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultDataForSubaddress]
        public void WhenLifetimeHasEndDate(
            IFixture fixture,
            AddressId addressId,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithLifetime(new CrabLifetime(importSubaddressPositionFromCrab.Lifetime.BeginDateTime.Value, importSubaddressPositionFromCrab.Lifetime.BeginDateTime.Value.PlusDays(1)));

            Assert(RetiredAddressScenario(fixture)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }
    }
}
