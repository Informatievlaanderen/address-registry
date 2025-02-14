namespace AddressRegistry.Tests.BackOffice.Lambda.WhenCorrectingAddressBoxNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Autofac;
    using AutoFixture;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IStreetNames _streetNames;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(789);
            var postalCode = Fixture.Create<PostalCode>();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                Fixture.Create<NisCode>());

            ProposeAddress(
                streetNamePersistentLocalId,
                parentAddressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            ProposeAddress(
                streetNamePersistentLocalId,
                childAddressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                new BoxNumber("1A"));

            var eTagResponse = new List<ETagResponse>();
            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(streetNamePersistentLocalId, new CorrectAddressBoxNumbersSqsRequest
                {
                    Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{childAddressPersistentLocalId}", Busnummer = "B" }] },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }),
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.First().ETag);
        }

        [Fact]
        public async Task WhenAddressHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new AddressHasInvalidStatusException(addressPersistentLocalId)).Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20"}]},
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        $"Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik': {addressPersistentLocalId}.",
                        "AdresIdGehistoreerdOfAfgekeurd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressHasNoBoxNumber_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new AddressHasNoBoxNumberException(addressPersistentLocalId)).Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        $"Het adres '{addressPersistentLocalId}' heeft geen te corrigeren busnummer.",
                        "AdresIdHuisnummerZonderBusnummer"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBoxNumberHasInvalidFormat_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<BoxNumberHasInvalidFormatException>().Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig busnummerformaat.",
                        "AdresOngeldigBusnummerformaat"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBoxNumberAlreadyExists_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var houseNumber = Fixture.Create<HouseNumber>();
            var boxNumber = Fixture.Create<BoxNumber>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new AddressAlreadyExistsException(houseNumber, boxNumber)).Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        $"Het huisnummer '{houseNumber}' in combinatie met busnummer '{boxNumber}' bestaat reeds voor de opgegeven straatnaam.",
                        "AdresBestaandeHuisnummerBusnummerCombinatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressIsRemovedException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new AddressIsRemovedException(addressPersistentLocalId)).Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(It.IsAny<Guid>(), new TicketError($"Verwijderd adres '{addressPersistentLocalId}'.", "VerwijderdAdresId"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenAddressIsNotFoundException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new AddressIsNotFoundException(addressPersistentLocalId)).Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(It.IsAny<Guid>(), new TicketError($"Onbestaand adres '{addressPersistentLocalId}'.", "AdresIdIsOnbestaand"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenBoxNumberHouseNumberDoesNotMatchParentHouseNumberException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()).Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest { Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = "http://a/1", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(It.IsAny<Guid>(), new TicketError("Lijst bevat verschillende huisnummers.", "VerschillendeHuisnummersNietToegestaanInLijstBusnummers"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenAggregateIdIsNotFound_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var sut = new CorrectAddressBoxNumbersLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AggregateIdIsNotFoundException>().Object);

            // Act
            await sut.Handle(new CorrectAddressBoxNumbersLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressBoxNumbersSqsRequest()
            {
                Request = new CorrectAddressBoxNumbersRequest{ Busnummers = [new CorrectAddressBoxNumbersRequestItem{ AdresId = $"http://a/{addressPersistentLocalId}", Busnummer = "20" }] },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Onbestaand adres.",
                        "404"),
                    CancellationToken.None));
        }
    }
}
