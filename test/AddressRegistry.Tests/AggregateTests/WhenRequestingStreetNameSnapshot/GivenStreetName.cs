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

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenStreetNameAndAddressesWereRemoved()
        {
            var command = Fixture.Create<CreateSnapshot>();

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Then(new Fact(
                    _streamId,
                    new StreetNameSnapshotWasRequested(Fixture.Create<StreetNamePersistentLocalId>()))));
        }
    }
}
