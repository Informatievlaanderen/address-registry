namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Api.BackOffice.Address;
    using AddressRegistry.Api.BackOffice.Address.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using Projections.Syndication.PostalInfo;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using StreetNameId = StreetName.StreetNameId;
    using HouseNumber = StreetName.HouseNumber;

    public class GivenStreetNameExists : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestConsumerContext _consumerContext;
        private readonly TestSyndicationContext _syndicationContext;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _consumerContext = new FakeConsumerContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTheAddressIsProposed()
        {
            const int expectedLocation = 5;
            var streetNameId = Fixture.Create<StreetNameId>();
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();

            //Arrange
            var consumerItem = _consumerContext
                .AddStreetNameConsumerItemFixtureWithPersistentLocalIdAndStreetNameId(streetNameId, streetNamePersistentId);
            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(expectedLocation));

            var streamVersion = 0;

            ImportMigratedStreetName(streetNameId, streetNamePersistentId);
            streamVersion++;

            string postInfoId = "8200";
            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                 PostalCode = postInfoId
            });
            _syndicationContext.SaveChanges();

            var body = new AddressProposeRequest
            {
                StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{consumerItem.PersistentLocalId}",
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = "11",
            };

            //Act
            var result = (CreatedWithLastObservedPositionAsETagResult)await _controller.Propose(
                ResponseOptions,
                _idempotencyContext,
                _backOfficeContext,
                mockPersistentLocalIdGenerator.Object,
                new AddressProposeRequestValidator(_syndicationContext),
                Container.Resolve<IStreetNames>(),
                body);

            //Assert
            result.Location.Should().Be(string.Format(DetailUrl, expectedLocation));
            result.LastObservedPositionAsETag.Length.Should().Be(128);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new StreetNameStreamId(streetNamePersistentId)), streamVersion, 1); //1 = version of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.LastObservedPositionAsETag);

            var municipalityIdByPersistentLocalId = await _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync(expectedLocation);
            municipalityIdByPersistentLocalId.Should().NotBeNull();
            municipalityIdByPersistentLocalId.StreetNamePersistentLocalId.Should().Be(consumerItem.PersistentLocalId);
        }
    }
}
