namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressRegularization
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
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

            var expectedIfMatchHeader = Fixture.Create<string>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressRegularizationSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult) await _controller.CorrectRegularization(
                MockIfMatchValidator(true),
                ifMatchHeaderValue: expectedIfMatchHeader,
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectAddressRegularizationSqsRequest>(sqsRequest =>
                        sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
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
        public void WithAggregateNotFoundException_ThenThrowsApiException()
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
        public void WhenAggregateIdIsNotFoundException_ThenApiException()
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
