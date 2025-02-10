namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressBoxNumbers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCorrectAddressBoxNumbersRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenCorrectAddressBoxNumbersRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressBoxNumbersSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var mockRequestValidator = new Mock<IValidator<CorrectAddressBoxNumbersRequest>>();
            mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<CorrectAddressBoxNumbersRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            var request = Fixture.Create<CorrectAddressBoxNumbersRequest>();

            var result = (AcceptedResult)await _controller.CorrectBoxNumbers(
                mockRequestValidator.Object,
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            mockRequestValidator.Invocations.Should().HaveCount(1);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectAddressBoxNumbersSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressBoxNumbersSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await _controller.CorrectBoxNumbers(
                MockValidRequestValidator<CorrectAddressBoxNumbersRequest>(),
                Fixture.Create<CorrectAddressBoxNumbersRequest>());

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
