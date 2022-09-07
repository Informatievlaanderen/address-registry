namespace AddressRegistry.Tests.ProjectionTests.Consumer
{
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Projections;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class StreetNameConsumerProjectionsTests : ConsumerProjectionTest<StreetNameConsumerProjection>
    {
        private readonly Fixture _fixture;
        private readonly ILogger _logger;

        public StreetNameConsumerProjectionsTests()
        {
            _logger = new LoggerFactory().CreateLogger(typeof(StreetNameConsumerProjectionsTests));
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedStreetNameId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
        }

        [Fact]
        public Task MigratedStreetNameWasImportedAddsStreetName()
        {
            var streetNameWasImported = _fixture.Create<MigratedStreetNameWasImported>();

            return Sut
                .Given(streetNameWasImported)
                .Then(async ct =>
                {
                    var expectedStreetName = await ct.StreetNameConsumerItems.FindAsync(streetNameWasImported.MunicipalityId);
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.StreetNameId.Should().Be(streetNameWasImported.StreetNameId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasImported.StreetNamePersistentLocalId);
                });
        }

        protected override StreetNameConsumerProjection CreateProjection()
            => new StreetNameConsumerProjection(_logger);
    }
}
