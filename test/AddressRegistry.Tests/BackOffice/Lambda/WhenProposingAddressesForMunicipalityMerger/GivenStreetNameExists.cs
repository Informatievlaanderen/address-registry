namespace AddressRegistry.Tests.BackOffice.Lambda.WhenProposingAddressesForMunicipalityMerger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Autofac;
    using AutoFixture;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Consumer.Read.Municipality.Projections;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Projections.Syndication.PostalInfo;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using StreetName.Exceptions;
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
            var setupData = Fixture.Create<SetupData>();

            var eTagResponses = new List<ETagResponse>();
            var ticketingMock = MockTicketing(result => { eTagResponses = result; });
            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                ticketingMock.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
                CancellationToken.None);

            // Assert
            var eTagResponse = eTagResponses.Single();
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(setupData.StreetNamePersistentLocalId))), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
            stream.Messages.First().JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());
        }

        [Fact]
        public async Task GivenRequestWithMultipleAddresses_ThenPersistentLocalIdETagResponses()
        {
            var setupData = Fixture.Create<SetupData>();
            var mergedAddressPersistentLocalId2 = Fixture.Create<AddressPersistentLocalId>();

            var eTagResponses = new List<ETagResponse>();
            var ticketingMock = MockTicketing(result => { eTagResponses = result; });
            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                ticketingMock.Object);

            await SetupMunicipalityAndStreetName(setupData);

            ProposeAddress(
                setupData.MergedStreetNamePersistentLocalId,
                mergedAddressPersistentLocalId2,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                null);

            // Act
            var request = CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData,
                extraAddresses: [
                    new ProposeAddressesForMunicipalityMergerSqsRequestItem(
                        setupData.PostInfoId,
                        Fixture.Create<AddressPersistentLocalId>(),
                        Fixture.Create<HouseNumber>(),
                        null,
                        setupData.MergedStreetNamePersistentLocalId,
                        mergedAddressPersistentLocalId2
                    )
                ]);
            await lambdaHandler.Handle(request, CancellationToken.None);

            // Assert
            eTagResponses.Should().HaveCount(2);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(setupData.StreetNamePersistentLocalId))), 2, 2);
            stream.Messages.First().JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());
            stream.Messages[0].JsonMetadata.Should().Contain(eTagResponses[1].ETag);
            stream.Messages[1].JsonMetadata.Should().Contain(eTagResponses[0].ETag);
        }

        [Fact]
        public async Task GivenDuplicateRequest_ThenPersistentLocalIdETagResponse()
        {
            var setupData = Fixture.Create<SetupData>();
            var houseNumber = Fixture.Create<HouseNumber>();
            var provenanceData = Fixture.Create<ProvenanceData>();

            await SetupMunicipalityAndStreetName(setupData);

            var eTagResponses = new List<ETagResponse>();
            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                MockTicketing(result => { eTagResponses = result; }).Object,
                Container.Resolve<IStreetNames>());

            var proposeAddressesForMunicipalityMergerLambdaRequest =
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData, houseNumber, provenanceData: provenanceData);

            // Act
            await lambdaHandler.Handle(proposeAddressesForMunicipalityMergerLambdaRequest, CancellationToken.None);
            await lambdaHandler.Handle(proposeAddressesForMunicipalityMergerLambdaRequest, CancellationToken.None);

            // Assert
            var eTagResponse = eTagResponses.Single();
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(setupData.StreetNamePersistentLocalId))), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var houseNumberRelation = await _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync((int)setupData.AddressPersistentLocalId);
            houseNumberRelation.Should().NotBeNull();
            houseNumberRelation!.StreetNamePersistentLocalId.Should().Be(setupData.StreetNamePersistentLocalId);
        }

        [Fact]
        public async Task WhenParentAddressAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<ParentAddressAlreadyExistsException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler(() => new HouseNumberHasInvalidFormatException("ABC")).Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

            var setupData = Fixture.Create<SetupData>();
            var houseNumber = Fixture.Create<HouseNumber>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler(() => new ParentAddressNotFoundException(setupData.StreetNamePersistentLocalId, HouseNumber.Create(houseNumber))).Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData, houseNumber),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{setupData.StreetNamePersistentLocalId}' en huisnummer '{houseNumber}'.",
                        "AdresActiefHuisNummerNietGekendValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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
        public async Task WhenPostalCodeMunicipalityDoesNotMatchStreetNameMunicipality_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometryMethodException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometrySpecificationException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

            var setupData = Fixture.Create<SetupData>();

            var lambdaHandler = CreateProposeAddressesForMunicipalityMergerLambdaHandler(
                MockExceptionIdempotentCommandHandler<BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException>().Object,
                ticketing.Object);

            await SetupMunicipalityAndStreetName(setupData);

            // Act
            await lambdaHandler.Handle(
                CreateProposeAddressesForMunicipalityMergerLambdaRequest(setupData),
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

        private async Task SetupMunicipalityAndStreetName(SetupData setupData)
        {
            _syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem { PostalCode = setupData.PostInfoId, NisCode = setupData.NisCode, });
            _municipalityContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem { MunicipalityId = setupData.MunicipalityId, NisCode = setupData.NisCode });
            await _municipalityContext.SaveChangesAsync();
            await _syndicationContext.SaveChangesAsync();

            ImportMigratedStreetName(new StreetNameId(Guid.NewGuid()), setupData.StreetNamePersistentLocalId, setupData.NisCode);
            ImportMigratedStreetName(new StreetNameId(Guid.NewGuid()), setupData.MergedStreetNamePersistentLocalId, setupData.NisCode);

            ProposeAddress(
                setupData.MergedStreetNamePersistentLocalId,
                setupData.MergedAddressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                null);
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
            SetupData setupData,
            HouseNumber? houseNumber = null,
            IEnumerable<ProposeAddressesForMunicipalityMergerSqsRequestItem>? extraAddresses = null,
            ProvenanceData? provenanceData = null)
        {
            return new ProposeAddressesForMunicipalityMergerLambdaRequest(
                setupData.StreetNamePersistentLocalId,
                new ProposeAddressesForMunicipalityMergerSqsRequest(
                    setupData.StreetNamePersistentLocalId,
                    new [] {
                        new ProposeAddressesForMunicipalityMergerSqsRequestItem(
                            setupData.PostInfoId,
                            setupData.AddressPersistentLocalId,
                            houseNumber ?? Fixture.Create<HouseNumber>(),
                            null,
                            setupData.MergedStreetNamePersistentLocalId,
                            setupData.MergedAddressPersistentLocalId
                        )
                    }.Concat(extraAddresses ?? []).ToList(),
                    provenanceData ?? Fixture.Create<ProvenanceData>()
                )
                {
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>()
                });
        }

        private sealed class SetupData
        {
            public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; set; }
            public StreetNamePersistentLocalId MergedStreetNamePersistentLocalId { get; set; }
            public AddressPersistentLocalId AddressPersistentLocalId { get; set; }
            public AddressPersistentLocalId MergedAddressPersistentLocalId  { get; set; }
            public MunicipalityId MunicipalityId { get; set; }
            public NisCode NisCode { get; set; }
            public PostalCode PostInfoId { get; set; }
        }
    }
}
