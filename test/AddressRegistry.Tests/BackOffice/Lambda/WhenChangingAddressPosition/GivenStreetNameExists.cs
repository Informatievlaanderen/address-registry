namespace AddressRegistry.Tests.BackOffice.Lambda.WhenChangingAddressPosition
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
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
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

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                new NisCode("12345"));

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var sut = new ChangeAddressPositionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            await sut.Handle(
                new ChangeAddressPositionLambdaRequest(streetNamePersistentLocalId, new ChangeAddressPositionSqsRequest
                {
                    Request = new ChangeAddressPositionRequest
                    {
                        PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                        PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                        Positie = GeometryHelpers.GmlPointGeometry
                    },
                    PersistentLocalId = addressPersistentLocalId,
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }),
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

            var sut = new ChangeAddressPositionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => Fixture.Create<AddressHasInvalidStatusException>()).Object);

            // Act
            await sut.Handle(
                new ChangeAddressPositionLambdaRequest(Fixture.Create<int>().ToString(), new ChangeAddressPositionSqsRequest
                    {
                        Request = new ChangeAddressPositionRequest
                        {
                            PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                            PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
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
                        "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.",
                        "AdresGehistoreerdOfAfgekeurd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressHasInvalidGeometryMethod_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ChangeAddressPositionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometryMethodException>().Object);

            // Act
            await sut.Handle(new ChangeAddressPositionLambdaRequest(Fixture.Create<int>().ToString(), new ChangeAddressPositionSqsRequest
            {
                Request = new ChangeAddressPositionRequest
                {
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie = PositieSpecificatie.Perceel,
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
                        "Ongeldige positieGeometrieMethode.",
                        "AdresPositieGeometriemethodeValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressHasInvalidGeometrySpecification_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ChangeAddressPositionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidGeometrySpecificationException>().Object);

            // Act
            await sut.Handle(new ChangeAddressPositionLambdaRequest(Fixture.Create<int>().ToString(), new ChangeAddressPositionSqsRequest
            {
                Request = new ChangeAddressPositionRequest
                {
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie = PositieSpecificatie.Perceel,
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
                        "Ongeldige positieSpecificatie.",
                        "AdresPositieSpecificatieValidatie"),
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
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

            var sut = new ChangeAddressPositionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                _streetNames,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            var streetName =
                await _streetNames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), CancellationToken.None);

            // Act
            await sut.Handle(new ChangeAddressPositionLambdaRequest(streetNamePersistentLocalId, new ChangeAddressPositionSqsRequest
                {
                    Request = new ChangeAddressPositionRequest
                    {
                        PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                        PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                        Positie = GeometryHelpers.GmlPointGeometry
                    },
                    PersistentLocalId = addressPersistentLocalId,
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
                            string.Format(ConfigDetailUrl, addressPersistentLocalId),
                            streetName.GetAddressHash(addressPersistentLocalId))),
                    CancellationToken.None));
        }
    }
}
