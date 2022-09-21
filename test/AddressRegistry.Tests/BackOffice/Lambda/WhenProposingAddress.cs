namespace AddressRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Projections.Syndication.PostalInfo;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;
    using MunicipalityLatestItem = Consumer.Read.Municipality.Projections.MunicipalityLatestItem;

    public class WhenProposingAddress : AddressRegistryBackOfficeTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly IStreetNames _streetNames;
        private readonly TestMunicipalityConsumerContext _municipalityContext;

        public WhenProposingAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            _municipalityContext = new FakeMunicipalityConsumerContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var nisCode = new NisCode("12345");
            var postInfoId = "2018";

            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(123));

            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                PostalCode = postInfoId,
                NisCode = nisCode,
            });
            _municipalityContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem
            {
                MunicipalityId = Fixture.Create<MunicipalityId>(),
                NisCode = nisCode
            });
            await _municipalityContext.SaveChangesAsync();
            await _syndicationContext.SaveChangesAsync();

            ImportMigratedStreetName(
                new AddressRegistry.StreetName.StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            var eTag = new ETagResponse(string.Empty);
            var sut = new SqsAddressProposeHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTag = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new SqsLambdaAddressProposeRequest
            {
                Request = new AddressBackOfficeProposeRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                MessageGroupId = streetNamePersistentLocalId,
                Metadata = new Dictionary<string, object>(),
                TicketId = Guid.NewGuid(),
                Provenance = Fixture.Create<Provenance>()
            },
                CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTag.LastEventHash);
        }
    }
}
