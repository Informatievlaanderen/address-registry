namespace AddressRegistry.Tests.BackOffice.Lambda.WhenProposingAddressesForMunicipalityMerger
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.AutoFixture;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using AddressRegistry.Tests.BackOffice.Lambda.Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();
            var municipalityId = Fixture.Create<MunicipalityId>();
//TODO-rik fix unit tests
            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var proposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                MockTicketing(result => { eTagResponse = result; }).Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, municipalityId, streetNamePersistentLocalId);

            // Act
            await proposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
            stream.Messages.First().JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());
        }

        [Fact]
        public async Task GivenDuplicateRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();
            var houseNumber = Fixture.Create<HouseNumber>();
            var provenanceData = Fixture.Create<ProvenanceData>();

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                MockTicketing(result => { eTagResponse = result; }).Object,
                Container.Resolve<IStreetNames>());

            var ProposeAddressesForMunicipalityMergerLambdaRequest =
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId, houseNumber, boxNumber: null, provenanceData);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(ProposeAddressesForMunicipalityMergerLambdaRequest, CancellationToken.None);
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(ProposeAddressesForMunicipalityMergerLambdaRequest, CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var houseNumberRelation = await _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync((int)addressPersistentLocalId);
            houseNumberRelation.Should().NotBeNull();
            houseNumberRelation.StreetNamePersistentLocalId.Should().Be(streetNamePersistentLocalId);
        }

        [Fact]
        public async Task WhenParentAddressAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<ParentAddressAlreadyExistsException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler(() => new HouseNumberHasInvalidFormatException("ABC")).Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Ongeldig huisnummerformaat.", "AdresOngeldigHuisnummerformaat"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBoxNumberAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();
            var houseNumber = Fixture.Create<HouseNumber>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler(() => new ParentAddressNotFoundException(streetNamePersistentLocalId, HouseNumber.Create(houseNumber))).Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId, houseNumber),
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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<StreetNameIsRemovedException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            var request = CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId);
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(request, CancellationToken.None);

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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometryMethodException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(CreateProposeAddressesForMunicipalityMergerLambdaRequest(
                streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

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

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometrySpecificationException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldige positieSpecificatie.",
                        "AdresPositieSpecificatieValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCode_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var ProposeAddressesForMunicipalityMergerLambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await ProposeAddressesForMunicipalityMergerLambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(streetNamePersistentLocalId, addressPersistentLocalId, postInfoId),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "De ingevoerde postcode komt niet overeen met de postcode van het huisnummer.",
                        "AdresPostinfoNietHetzelfdeAlsHuisnummer"),
                    CancellationToken.None));
        }

        private async Task SetupMunicipalityAndStreetName(PostalCode postInfoId, NisCode nisCode, MunicipalityId municipalityId, StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem { PostalCode = postInfoId, NisCode = nisCode, });
            _municipalityContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem { MunicipalityId = municipalityId, NisCode = nisCode });
            await _municipalityContext.SaveChangesAsync();
            await _syndicationContext.SaveChangesAsync();

            ImportMigratedStreetName(new StreetNameId(Guid.NewGuid()), streetNamePersistentLocalId, nisCode);
        }

        private ProposeAddressesForMunicipalityMergerLambdaHandler CreateProposeAddressesForMunicipalityMergerLambdaHandler(
            IIdempotentCommandHandler idempotentCommandHandler,
            ITicketing ticketing,
            IStreetNames? streetNames = null)
        {
            return new ProposeAddressesForMunicipalityMergerLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing,
                idempotentCommandHandler,
                _backOfficeContext,
                streetNames ?? _streetNames);
        }

        private ProposeAddressesForMunicipalityMergerLambdaRequest CreateProposeAddressesForMunicipalityMergerLambdaRequest(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postInfoId,
            HouseNumber? houseNumber = null,
            BoxNumber? boxNumber = null,
            ProvenanceData? provenanceData = null)
        {
            return new ProposeAddressesForMunicipalityMergerLambdaRequest(
                streetNamePersistentLocalId,
                new ProposeAddressesForMunicipalityMergerSqsRequest
                {
                    NisCode = Fixture.Create<string>(),
                    Addresses = Fixture.CreateMany<ProposeAddressesForMunicipalityMergerSqsRequestItem>(5).ToList(),
                    // PersistentLocalId = addressPersistentLocalId,
                    // Request = new ProposeAddressesForMunicipalityMergerRequest
                    // {
                    //     StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                    //     PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                    //     Huisnummer = houseNumber ?? Fixture.Create<HouseNumber>(),
                    //     Busnummer = boxNumber,
                    //     PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    //     PositieSpecificatie = PositieSpecificatie.Ingang,
                    //     Positie = GeometryHelpers.GmlPointGeometry
                    // },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = provenanceData ?? Fixture.Create<ProvenanceData>()
                });
        }
    }
}
