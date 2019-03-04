namespace AddressRegistry.Tests.WhenImportSubaddressMailCantonFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using Crab;
    using global::AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        //ImportSubaddressMailCantonFromCrab routes to ImportHouseNumberMailCantonFromCrab which is already tested
        //Only one "happy path" test here, to check if the command is correctly routed

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressPostalCodeWasChanged(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressMailCantonFromCrab importSubaddressMailCantonFromCrab
        )
        {
            Assert(RegisteredAddressScenario(fixture)
                .When(importSubaddressMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasChanged(addressId, new PostalCode(importSubaddressMailCantonFromCrab.MailCantonCode)),
                    importSubaddressMailCantonFromCrab.ToLegacyEvent()));
        }
    }
}
