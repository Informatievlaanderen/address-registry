namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressPosition
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using StreetName;
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled  : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressPositionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            var result = (AcceptedResult)await _controller.CorrectPosition(MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(true),
                addressPersistentLocalId,
                request: new CorrectAddressPositionRequest
                {
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = await _controller.CorrectPosition(MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(false),
                addressPersistentLocalId,
                request: new CorrectAddressPositionRequest
                {
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                ifMatchHeaderValue: null);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task WithAddressIsNotFoundException_ThenThrowsApiException()
        {
            Func<Task> act = async () => await _controller.CorrectPosition(MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidatorThrowsAddressIsNotFoundException(),
                Fixture.Create<AddressPersistentLocalId>(),
                request: new CorrectAddressPositionRequest
                {
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                    Positie = GeometryHelpers.GmlPointGeometry
                },
                ifMatchHeaderValue: null);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand adres.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressPositionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await _controller.CorrectPosition(MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(true),
                addressPersistentLocalId,
                new CorrectAddressPositionRequest(),
                string.Empty);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand adres.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }
    }
}
