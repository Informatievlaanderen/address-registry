namespace AddressRegistry.Tests.AggregateTests.WhenRequestingStreetNameSnapshot
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

    public class GivenNoStreetNameExists : AddressRegistryTest
    {

        public GivenNoStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenStreetNameAndAddressesWereRemoved()
        {
            var command = Fixture.Create<CreateSnapshot>();
            var streamId = new StreetNameStreamId(command.StreetNamePersistentLocalId);

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new AggregateNotFoundException(streamId, typeof(StreetName))));
        }
    }
}
