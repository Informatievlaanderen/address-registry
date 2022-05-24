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
    using HouseNumber = StreetName.HouseNumber;
    using PostalCode = StreetName.PostalCode;
    using BoxNumber = StreetName.BoxNumber;
    using StreetNameId = StreetName.StreetNameId;

    public class GivenChildAddressAlreadyExists : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestConsumerContext _consumerContext;
        private readonly TestSyndicationContext _syndicationContext;

        public GivenChildAddressAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _consumerContext = new FakeConsumerContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            const int expectedLocation = 5;
            string postInfoId = "8200";
            string houseNumber = "11";
            string boxNumber = "1A";
            var streetNameId = Fixture.Create<StreetNameId>();
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();

            //Arrange
            var consumerItem = _consumerContext
                .AddStreetNameConsumerItemFixtureWithPersistentLocalIdAndStreetNameId(streetNameId, streetNamePersistentId);
            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(expectedLocation));

            ImportMigratedStreetName(streetNameId, streetNamePersistentId);
            ProposeAddress(streetNamePersistentId, new AddressPersistentLocalId(123), new PostalCode(postInfoId), new HouseNumber(houseNumber), null);
            ProposeAddress(streetNamePersistentId, new AddressPersistentLocalId(123), new PostalCode(postInfoId), new HouseNumber(houseNumber), new BoxNumber(boxNumber));

            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                 PostalCode = postInfoId
            });
            _syndicationContext.SaveChanges();

            var body = new AddressProposeRequest
            {
                StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{consumerItem.PersistentLocalId}",
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = houseNumber,
                Busnummer = boxNumber
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
                        failure => failure.ErrorCode == "AdresBestaandeHuisnummerBusnummerCombinatie"
                                   && failure.ErrorMessage == "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam."
                                   && failure.PropertyName == nameof(body.Busnummer)));
        }
    }
}
