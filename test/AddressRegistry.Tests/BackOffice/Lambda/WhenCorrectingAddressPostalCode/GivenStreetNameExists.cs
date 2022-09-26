namespace AddressRegistry.Tests.BackOffice.Lambda.WhenCorrectingAddressPostalCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Exceptions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using Projections.Syndication.PostalInfo;
    using StreetName;
    using StreetName.Exceptions;
    using AutoFixture;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly TestMunicipalityConsumerContext _municipalityContext;
        private readonly IStreetNames _streetNames;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

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

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var sut = new SqsAddressCorrectPostalCodeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _syndicationContext,
                _municipalityContext);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectPostalCodeRequest
            {
                Request = new AddressBackOfficeCorrectPostalCodeRequest
                {
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{correctPostInfoId}"
                },
                AddressPersistentLocalId = addressPersistentLocalId,
                MessageGroupId = streetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenAddressHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
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

            var sut = new SqsAddressCorrectPostalCodeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidStatusException>().Object,
                _syndicationContext,
                _municipalityContext);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectPostalCodeRequest
            {
                Request = new AddressBackOfficeCorrectPostalCodeRequest
                {
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}"
                },
                AddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>(),
                MessageGroupId = Fixture.Create<int>().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.",
                        "AdresGehistoreerdOfAfgekeurd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenPostalCodeMunicipalityDoesNotMatchStreetNameMunicipality_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
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

            var sut = new SqsAddressCorrectPostalCodeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException>().Object,
                _syndicationContext,
                _municipalityContext);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectPostalCodeRequest
            {
                Request = new AddressBackOfficeCorrectPostalCodeRequest
                {
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}"
                },
                AddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>(),
                MessageGroupId = Fixture.Create<int>().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.",
                        "AdresPostinfoNietInGemeente"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);
            var nisCode = new NisCode("12345");
            var houseNumber = new HouseNumber("11");
            var postInfoId = "2018";
            var correctPostInfoId = "2019";
            var municipalityId = Fixture.Create<MunicipalityId>();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                new PostalCode(postInfoId),
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

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

            var sut = new SqsAddressCorrectPostalCodeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                _streetNames,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                _syndicationContext,
                _municipalityContext);

            var streetName =
                await _streetNames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), CancellationToken.None);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectPostalCodeRequest
            {
                Request = new AddressBackOfficeCorrectPostalCodeRequest
                {
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}"
                },
                AddressPersistentLocalId = addressPersistentLocalId,
                MessageGroupId = streetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            },
            CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, addressPersistentLocalId),
                            streetName.GetAddressHash(addressPersistentLocalId))),
                    CancellationToken.None));
        }
    }
}
