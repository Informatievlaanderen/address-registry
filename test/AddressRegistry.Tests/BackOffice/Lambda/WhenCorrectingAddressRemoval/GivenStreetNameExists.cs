namespace AddressRegistry.Tests.BackOffice.Lambda.WhenCorrectingAddressRemoval
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
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithValidHouseNumber());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                Fixture.Create<NisCode>());

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                null);

            RemoveAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            await sut.Handle(new CorrectAddressRemovalLambdaRequest(streetNamePersistentLocalId.ToString(), new CorrectAddressRemovalSqsRequest
                {
                    Request = new CorrectAddressRemovalRequest { PersistentLocalId = addressPersistentLocalId },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }),
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenStreetNameIsRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<StreetNameIsRemovedException>().Object);

            // Act
            var request = new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest
            {
                Request = new CorrectAddressRemovalRequest(),
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
                        "Verwijderde straatnaam.",
                        "VerwijderdeStraatnaam"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object);

            // Act
            var request = new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest()
            {
                Request = new CorrectAddressRemovalRequest(),
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
                        "Deze actie is enkel toegestaan binnen straatnamen met status 'voorgesteld' of 'inGebruik'.",
                        "AdresStraatnaamVoorgesteldOfInGebruik"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressBoxNumberHasInconsistentHouseNumberException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<BoxNumberHouseNumberDoesNotMatchParentHouseNumberException>().Object);

            // Act
            await sut.Handle(new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest()
            {
                Request = new CorrectAddressRemovalRequest { PersistentLocalId = Fixture.Create<AddressPersistentLocalId>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is niet toegestaan op een busnummer wegens een inconsistent huisnummer.",
                        "AdresBusnummerHuisnummerInconsistent"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressAlreadyExistsException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object);

            // Act
            await sut.Handle(
                new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest()
                {
                    Request = new CorrectAddressRemovalRequest{ PersistentLocalId = Fixture.Create<int>() },
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
        public async Task WhenParentAddressIsRemovedException_ThenTicketingErrorIsExpected()
        {
            Fixture.Customize(new WithFixedValidHouseNumber());

            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => Fixture.Create<ParentAddressIsRemovedException>()).Object);

            // Act
            await sut.Handle(new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest
            {
                Request = new CorrectAddressRemovalRequest { PersistentLocalId = Fixture.Create<int>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Verwijderd huisnummeradres.",
                        "VerwijderdHuisnummerAdres"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenParentAddressHasInvalidStatusException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<ParentAddressHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest
            {
                Request = new CorrectAddressRemovalRequest { PersistentLocalId = Fixture.Create<int>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "TODO",
                        "AdresHuisnummerTODO"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressBoxNumberHasInconsistentPostalCodeException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException>().Object);

            // Act
            await sut.Handle(new CorrectAddressRemovalLambdaRequest(Fixture.Create<int>().ToString(), new CorrectAddressRemovalSqsRequest
            {
                Request = new CorrectAddressRemovalRequest { PersistentLocalId = Fixture.Create<int>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is niet toegestaan op een busnummer wegens een inconsistente postcode.",
                        "AdresBusnummerPostcodeInconsistent"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<NisCode>());

            ProposeAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                null);

            ApproveAddress(Fixture.Create<StreetNamePersistentLocalId>(), Fixture.Create<AddressPersistentLocalId>());

            RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>());

            var sut = new CorrectAddressRemovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                _streetNames,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            var streetName =
                await _streetNames.GetAsync(new StreetNameStreamId(Fixture.Create<StreetNamePersistentLocalId>()), CancellationToken.None);

            // Act
            await sut.Handle(new CorrectAddressRemovalLambdaRequest(
                    Fixture.Create<StreetNamePersistentLocalId>(),
                        new CorrectAddressRemovalSqsRequest
                        {
                            Request = new CorrectAddressRemovalRequest{ PersistentLocalId = Fixture.Create<AddressPersistentLocalId>() },
                            TicketId = Guid.NewGuid(),
                            Metadata = new Dictionary<string, object?>(),
                            ProvenanceData = Fixture.Create<ProvenanceData>()
                        }),
            CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, Fixture.Create<AddressPersistentLocalId>()),
                            streetName.GetAddressHash(Fixture.Create<AddressPersistentLocalId>()))),
                    CancellationToken.None));
        }
    }
}
