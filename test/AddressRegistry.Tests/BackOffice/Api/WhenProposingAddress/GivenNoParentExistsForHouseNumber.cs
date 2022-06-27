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
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using StreetNameId = StreetName.StreetNameId;

    public class GivenNoParentExistsForHouseNumber : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestConsumerContext _consumerContext;
        private readonly TestSyndicationContext _syndicationContext;

        public GivenNoParentExistsForHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _consumerContext = new FakeConsumerContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            const int expectedLocation = 5;
            string postInfoId = "8200";
            string houseNumber = "11";
            string boxNumber = "1A";
            var nisCode = Fixture.Create<NisCode>();
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

            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                 PostalCode = postInfoId,
                 NisCode = nisCode
            });

            _syndicationContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem
            {
                MunicipalityId = Fixture.Create<MunicipalityId>(),
                NisCode = nisCode
            });
            _syndicationContext.SaveChanges();

            var streetNamePuri = $"https://data.vlaanderen.be/id/straatnaam/{consumerItem.PersistentLocalId}";
            var postInfoPuri = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}";
            var body = new AddressProposeRequest
            {
                StraatNaamId = streetNamePuri,
                PostInfoId = postInfoPuri,
                Huisnummer = houseNumber,
                Busnummer = boxNumber
            };

            //Act
            Func<Task> act = async () => await _controller.Propose(
                ResponseOptions,
                _idempotencyContext,
                _backOfficeContext,
                _syndicationContext,
                mockPersistentLocalIdGenerator.Object,
                new AddressProposeRequestValidator(_syndicationContext),
                Container.Resolve<IStreetNames>(),
                body);

            // Assert
            var d = act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result;
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(
                        failure => failure.ErrorCode == "AdresActiefHuisNummerNietGekendValidatie"
                                   && failure.ErrorMessage == $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{streetNamePuri}' en huisnummer '{houseNumber}'."
                                   && failure.PropertyName == nameof(body.Huisnummer)));
        }
    }
}
