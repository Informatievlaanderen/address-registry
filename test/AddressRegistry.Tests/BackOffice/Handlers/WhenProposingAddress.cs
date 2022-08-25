namespace AddressRegistry.Tests.BackOffice.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using Infrastructure;
    using Moq;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenProposingAddress : AddressRegistryBackOfficeTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly IStreetNames _streetNames;

        public WhenProposingAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var niscode = new NisCode("12345");
            var postInfoId = "2018";

            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(123));
            
            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                PostalCode = postInfoId,
                NisCode = niscode,
            });
            _syndicationContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem
            {
                MunicipalityId = Fixture.Create<MunicipalityId>(),
                NisCode = niscode
            });
            _syndicationContext.SaveChanges();

            ImportMigratedStreetName(
                new AddressRegistry.StreetName.StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                niscode);

            var sut = new AddressProposeHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                _streetNames,
                _backOfficeContext,
                _idempotencyContext,
                _syndicationContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            var result = await sut.Handle(new AddressProposeRequest
            {
                StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = "11",
                Busnummer = null,
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = GeometryHelpers.PointGeometry,
                Metadata = new Dictionary<string, object>()
            },
                CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 1, 1); //1 = version of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.LastEventHash);
        }
    }
}
