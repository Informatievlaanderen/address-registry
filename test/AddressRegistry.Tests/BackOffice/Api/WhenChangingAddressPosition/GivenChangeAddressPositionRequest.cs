namespace AddressRegistry.Tests.BackOffice.Api.WhenChangingAddressPosition
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenChangeAddressPositionRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenChangeAddressPositionRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeAddressPositionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.ChangePosition(
                MockValidRequestValidator<ChangeAddressPositionRequest>(),
                MockIfMatchValidator(true),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<ChangeAddressPositionRequest>(),
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.ChangePosition(
                MockValidRequestValidator<ChangeAddressPositionRequest>(),
                MockIfMatchValidator(false),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<ChangeAddressPositionRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task WithAddressIsNotFoundException_ThenThrowsApiException()
        {
            Func<Task> act = async () => await  _controller.ChangePosition(
                MockValidRequestValidator<ChangeAddressPositionRequest>(),
                MockIfMatchValidatorThrowsAddressIsNotFoundException(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<ChangeAddressPositionRequest>(),
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
        public async Task WithAggregateNotFoundException_ThenThrowsApiException()
        {
            Func<Task> act = async () => await  _controller.ChangePosition(
                MockValidRequestValidator<ChangeAddressPositionRequest>(),
                MockIfMatchValidatorThrowsAggregateNotFoundException(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<ChangeAddressPositionRequest>(),
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
            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeAddressPositionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await _controller.ChangePosition(
                MockValidRequestValidator<ChangeAddressPositionRequest>(),
                MockIfMatchValidator(true),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<ChangeAddressPositionRequest>(),
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
    }
}
