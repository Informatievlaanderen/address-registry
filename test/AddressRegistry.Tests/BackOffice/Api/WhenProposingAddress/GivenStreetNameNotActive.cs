namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Api.BackOffice.Address;
    using AddressRegistry.Api.BackOffice.Address.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using Projections.Syndication.PostalInfo;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using StreetNameId = StreetName.StreetNameId;

    public class GivenStreetNameNotActive : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestConsumerContext _consumerContext;
        private readonly TestSyndicationContext _syndicationContext;

        public GivenStreetNameNotActive(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _consumerContext = new FakeConsumerContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public async Task ThenThrowValidationException(StreetNameStatus streetNameStatus)
        {
            const int expectedLocation = 5;
            string postInfoId = "8200";
            string houseNumber = "11";
            var streetNameId = Fixture.Create<StreetNameId>();
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();

            //Arrange
            var consumerItem = _consumerContext
                .AddStreetNameConsumerItemFixtureWithPersistentLocalIdAndStreetNameId(streetNameId, streetNamePersistentId);
            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(expectedLocation));

            ImportMigratedStreetName(streetNameId, streetNamePersistentId, streetNameStatus);

            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                 PostalCode = postInfoId
            });
            _syndicationContext.SaveChanges();

            var body = new AddressProposeRequest
            {
                StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{consumerItem.PersistentLocalId}",
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = houseNumber
            };

            //Act
            Func<Task> act = async () => await _controller.Propose(
                ResponseOptions,
                _idempotencyContext,
                _backOfficeContext,
                mockPersistentLocalIdGenerator.Object,
                new AddressProposeRequestValidator(_syndicationContext),
                Container.Resolve<IStreetNames>(),
                body);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(
                        failure => failure.ErrorCode == "AdresStraatnaamGehistoreerdOfAfgekeurd"
                                   && failure.ErrorMessage == "De straatnaam is gehistoreerd of afgekeurd."
                                   && failure.PropertyName == nameof(body.StraatNaamId)));
        }
    }
}
