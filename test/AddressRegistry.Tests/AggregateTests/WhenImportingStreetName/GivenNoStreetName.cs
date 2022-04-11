namespace AddressRegistry.Tests.AggregateTests.WhenImportingStreetName
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoStreetName : AddressRegistryTest
    {
        public GivenNoStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenStreetNameWasImported()
        {
            var command = Fixture.Create<ImportStreetName>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasImported(command.PersistentLocalId, command.MunicipalityId, command.StreetNameStatus))));
        }
    }
}
