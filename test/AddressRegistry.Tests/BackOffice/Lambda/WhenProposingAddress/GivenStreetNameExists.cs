namespace AddressRegistry.Tests.BackOffice.Lambda.WhenProposingAddress
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Projections.Syndication.PostalInfo;
    using StreetName;
    using StreetName.Exceptions;
    using AutoFixture;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;
    using MunicipalityLatestItem = Consumer.Read.Municipality.Projections.MunicipalityLatestItem;
    using HouseNumber = StreetName.HouseNumber;

    public class GivenStreetNameExists : BackOfficeLambdaTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly IStreetNames _streetNames;
        private readonly TestMunicipalityConsumerContext _municipalityContext;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new ProposeAddressLambdaRequest(streetNamePersistentLocalId, new ProposeAddressSqsRequest()
                {
                    Request = new ProposeAddressRequest
                    {
                        StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                        PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                        Huisnummer = "11",
                        Busnummer = null,
                        PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                        PositieSpecificatie = PositieSpecificatie.Ingang,
                        Positie = GeometryHelpers.GmlPointGeometry
                    },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }),
                CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenParentAddressAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<ParentAddressAlreadyExistsException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest()
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.",
                        "AdresBestaandeHuisnummerBusnummerCombinatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenHouseNumberHasInvalidFormat_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<HouseNumberHasInvalidFormatException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig huisnummerformaat.",
                        "AdresOngeldigHuisnummerformaat"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBoxNumberAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.",
                        "AdresBestaandeHuisnummerBusnummerCombinatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenParentAddressNotFound_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var nisCode = new NisCode("12345");
            var postInfoId = "2018";
            var houseNumber = "11";

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() =>
                    new ParentAddressNotFoundException(streetNamePersistentLocalId, HouseNumber.Create(houseNumber))).Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest()
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = houseNumber,
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{StraatNaamPuri}{streetNamePersistentLocalId}' en huisnummer '{houseNumber}'.",
                        "AdresActiefHuisNummerNietGekendValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            await sut.Handle(new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest()
                {
                    Request = new ProposeAddressRequest
                    {
                        StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                        PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                        Huisnummer = "11",
                        Busnummer = null,
                        PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                        PositieSpecificatie = PositieSpecificatie.Ingang,
                        Positie = GeometryHelpers.GmlPointGeometry
                    },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "De straatnaam is gehistoreerd of afgekeurd.",
                        "AdresStraatnaamGehistoreerdOfAfgekeurd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameIsRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<StreetNameIsRemovedException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            var request = new ProposeAddressLambdaRequest(streetNamePersistentLocalId, new ProposeAddressSqsRequest()
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });
            await sut.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        $"De straatnaam '{StraatNaamPuri}{request.StreetNamePersistentLocalId()}' is niet gekend in het straatnaamregister.",
                        "AdresStraatnaamNietGekendValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenPostalCodeMunicipalityDoesNotMatchStreetNameMunicipality_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            var request = new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest()
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });
            await sut.Handle(request, CancellationToken.None);

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
        public async Task WhenInvalidGeometryMethod_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometryMethodException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            var request = new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest()
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });
            await sut.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldige positieGeometrieMethode.",
                        "AdresPositieGeometriemethodeValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenInvalidGeometrySpecification_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

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

            var sut = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometrySpecificationException>().Object,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                mockPersistentLocalIdGenerator.Object);

            // Act
            var request = new ProposeAddressLambdaRequest(Fixture.Create<int>().ToString(), new ProposeAddressSqsRequest()
            {
                Request = new ProposeAddressRequest
                {
                    StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    Huisnummer = "11",
                    Busnummer = null,
                    PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    PositieSpecificatie = PositieSpecificatie.Ingang,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });
            await sut.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldige positieSpecificatie.",
                        "AdresPositieSpecificatieValidatie"),
                    CancellationToken.None));
        }
    }
}
