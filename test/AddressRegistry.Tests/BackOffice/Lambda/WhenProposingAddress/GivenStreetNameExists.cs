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
    using BoxNumber = StreetName.BoxNumber;
    using MunicipalityLatestItem = Consumer.Read.Municipality.Projections.MunicipalityLatestItem;
    using HouseNumber = StreetName.HouseNumber;
    using PostalCode = StreetName.PostalCode;

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();
            var municipalityId = Fixture.Create<MunicipalityId>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                persistentLocalIdGenerator.Object,
                MockTicketing(result => { eTagResponse = result; }).Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, municipalityId, streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();
            var houseNumber = Fixture.Create<HouseNumber>();
            var boxNumber = Fixture.Create<BoxNumber>();
            var provenanceData = Fixture.Create<ProvenanceData>();

            var houseNumberPersistentLocalId = new PersistentLocalId(1);
            var boxNumberPersistentLocalId = new PersistentLocalId(2);
            var duplicatePersistentLocalId = new PersistentLocalId(3);
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .SetupSequence(x => x.GenerateNextPersistentLocalId())
                .Returns(houseNumberPersistentLocalId)
                .Returns(boxNumberPersistentLocalId)
                .Returns(duplicatePersistentLocalId);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await using var scope1 = Container.BeginLifetimeScope();
            await CreateProposeAddressLambdaHandler(
                new IdempotentCommandHandler(scope1.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                persistentLocalIdGenerator.Object,
                MockTicketing(_ => {}).Object,
                scope1.Resolve<IStreetNames>()).Handle(
                CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId, houseNumber, boxNumber: null, provenanceData), CancellationToken.None);

            await using var scope2 = Container.BeginLifetimeScope();
            var firstTagResponse = new ETagResponse("", "");
            await CreateProposeAddressLambdaHandler(
                new IdempotentCommandHandler(scope2.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                persistentLocalIdGenerator.Object,
                MockTicketing(result => { firstTagResponse = result; }).Object,
                scope2.Resolve<IStreetNames>()).Handle(
                CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId, houseNumber, boxNumber: boxNumber, provenanceData), CancellationToken.None);

            await using var scope3 = Container.BeginLifetimeScope();
            var duplicateTagResponse = new ETagResponse("", "");
            await CreateProposeAddressLambdaHandler(
                new IdempotentCommandHandler(scope3.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                persistentLocalIdGenerator.Object,
                MockTicketing(result => { duplicateTagResponse = result; }).Object,
                scope3.Resolve<IStreetNames>()).Handle(
                CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId, houseNumber, boxNumber: boxNumber, provenanceData), CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(firstTagResponse.ETag);

            firstTagResponse.ETag.Should().Be(duplicateTagResponse.ETag);

            var houseNumberRelation = await _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync((int)houseNumberPersistentLocalId);
            houseNumberRelation.Should().NotBeNull();
            houseNumberRelation.StreetNamePersistentLocalId.Should().Be(streetNamePersistentLocalId);
            var boxNumberRelation = await _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync((int)boxNumberPersistentLocalId);
            boxNumberRelation.Should().NotBeNull();
            boxNumberRelation.StreetNamePersistentLocalId.Should().Be(streetNamePersistentLocalId);

            (await _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync((int)duplicatePersistentLocalId)).Should().BeNull();
        }

        [Fact]
        public async Task WhenParentAddressAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<ParentAddressAlreadyExistsException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<HouseNumberHasInvalidFormatException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();
            var houseNumber = Fixture.Create<HouseNumber>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler(() => new ParentAddressNotFoundException(streetNamePersistentLocalId, HouseNumber.Create(houseNumber))).Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId, houseNumber), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<StreetNameIsRemovedException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            var request = CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId);
            await proposeAddressLambdaHandler.Handle(request, CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometryMethodException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

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
            var nisCode = Fixture.Create<NisCode>();
            var postInfoId = Fixture.Create<PostalCode>();

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var proposeAddressLambdaHandler = CreateProposeAddressLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometrySpecificationException>().Object,
                persistentLocalIdGenerator.Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(postInfoId, nisCode, Fixture.Create<MunicipalityId>(), streetNamePersistentLocalId);

            // Act
            await proposeAddressLambdaHandler.Handle(CreateProposeAddressLambdaRequest(streetNamePersistentLocalId, postInfoId), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldige positieSpecificatie.",
                        "AdresPositieSpecificatieValidatie"),
                    CancellationToken.None));
        }

        private async Task SetupMunicipalityAndStreetName(PostalCode postInfoId, NisCode nisCode, MunicipalityId municipalityId, StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem { PostalCode = postInfoId, NisCode = nisCode, });
            _municipalityContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem { MunicipalityId = municipalityId, NisCode = nisCode });
            await _municipalityContext.SaveChangesAsync();
            await _syndicationContext.SaveChangesAsync();

            ImportMigratedStreetName(new AddressRegistry.StreetName.StreetNameId(Guid.NewGuid()), streetNamePersistentLocalId, nisCode);
        }

        private ProposeAddressLambdaHandler CreateProposeAddressLambdaHandler(
            IIdempotentCommandHandler idempotentCommandHandler,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            ITicketing ticketing,
            IStreetNames? streetNames = null)
        {
            var proposeAddressLambdaHandler = new ProposeAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing,
                streetNames ?? _streetNames,
                idempotentCommandHandler,
                _backOfficeContext,
                _syndicationContext,
                _municipalityContext,
                persistentLocalIdGenerator);
            return proposeAddressLambdaHandler;
        }

        private ProposeAddressLambdaRequest CreateProposeAddressLambdaRequest(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postInfoId,
            HouseNumber? houseNumber = null,
            BoxNumber? boxNumber = null,
            ProvenanceData? provenanceData = null)
        {
            return new ProposeAddressLambdaRequest(
                streetNamePersistentLocalId,
                new ProposeAddressSqsRequest
                {
                    Request = new ProposeAddressRequest
                    {
                        StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentLocalId}",
                        PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                        Huisnummer = houseNumber ?? Fixture.Create<HouseNumber>(),
                        Busnummer = boxNumber,
                        PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                        PositieSpecificatie = PositieSpecificatie.Ingang,
                        Positie = GeometryHelpers.GmlPointGeometry
                    },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = provenanceData ?? Fixture.Create<ProvenanceData>()
                });
        }
    }
}
