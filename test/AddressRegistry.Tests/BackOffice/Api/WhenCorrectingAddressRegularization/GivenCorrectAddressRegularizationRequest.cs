namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressRegularization
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
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
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenCorrectAddressRegularizationRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenCorrectAddressRegularizationRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        // Happy path
        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressRegularizationSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult) await _controller.CorrectRegularization(
                MockIfMatchValidator(true),
                ifMatchHeaderValue: null,
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var result = await _controller.CorrectRegularization(
                MockIfMatchValidator(false),
                "OUTDATED",
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task WithAddressIsNotFoundException_ThenThrowsApiException()
        {
            Func<Task> act = async () => await _controller.CorrectRegularization(
                MockIfMatchValidatorThrowsAddressIsNotFoundException(),
                ifMatchHeaderValue: null,
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

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
            Func<Task> act = async () => await _controller.CorrectRegularization(
                MockIfMatchValidatorThrowsAggregateNotFoundException(),
                ifMatchHeaderValue: null,
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand adres.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task WhenAggregateIdIsNotFoundException_ThenApiException()
        {
            //Arrange
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressRegularizationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            //Act
            Func<Task> act = async () => await _controller.CorrectRegularization(
                MockIfMatchValidator(true),
                ifMatchHeaderValue: null,
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

            // Assert
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
