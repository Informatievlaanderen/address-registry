namespace AddressRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Consumer.Read.Municipality.Projections;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Projections.Syndication.PostalInfo;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenCorrectingAddressPostalCode : AddressRegistryBackOfficeTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly TestMunicipalityConsumerContext _municipalityContext;
        private readonly IStreetNames _streetNames;

        public WhenCorrectingAddressPostalCode(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            _municipalityContext = new FakeMunicipalityConsumerContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);

            var postInfoId = "2018";
            var correctPostInfoId = "2019";
            var nisCode = Fixture.Create<NisCode>();
            var municipalityId = Fixture.Create<MunicipalityId>();

            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                PostalCode = postInfoId,
                NisCode = nisCode
            });
            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                PostalCode = correctPostInfoId,
                NisCode = nisCode
            });
            await _syndicationContext.SaveChangesAsync();

            _municipalityContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem
            {
                MunicipalityId = municipalityId,
                NisCode = nisCode
            });
            await _municipalityContext.SaveChangesAsync();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                new NisCode("12345"));

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                new PostalCode(postInfoId),
                municipalityId,
                new HouseNumber("11"),
                null);

            ETagResponse? etag = null;

            var sut = new SqsAddressCorrectPostalCodeHandler(
                MockTicketing(result =>
                {
                    etag = result;
                }).Object,
                MockTicketingUrl().Object,
                Container.Resolve<ICommandHandlerResolver>(),
                _streetNames,
                _idempotencyContext,
                _syndicationContext,
                _municipalityContext);

            // Act
            await sut.Handle(new SqsAddressCorrectPostalCodeRequest
            {
                PersistentLocalId = addressPersistentLocalId,
                MessageGroupId = streetNamePersistentLocalId,
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{correctPostInfoId}",
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag!.LastEventHash);
        }
    }
}
