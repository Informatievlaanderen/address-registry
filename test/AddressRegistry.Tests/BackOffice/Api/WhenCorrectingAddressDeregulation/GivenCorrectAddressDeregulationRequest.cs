namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressDeregulation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetName;
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
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCorrectAddressDeregulationRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenCorrectAddressDeregulationRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            var expectedIfMatchHeader = Fixture.Create<string>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressDeregulationSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.CorrectDeregulation(
                MockIfMatchValidator(true),
                ifMatchHeaderValue: expectedIfMatchHeader,
                Fixture.Create<AddressPersistentLocalId>());

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectAddressDeregulationSqsRequest>(sqsRequest =>
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
            //Act
            var result = await _controller.CorrectDeregulation(
                MockIfMatchValidator(false),
                "IncorrectIfMatchHeader",
                Fixture.Create<AddressPersistentLocalId>());

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WithAggregateNotFoundException_ThenThrowsApiException()
        {
            Func<Task> act = async () => await  _controller.CorrectDeregulation(
                MockIfMatchValidatorThrowsAggregateNotFoundException(),
                ifMatchHeaderValue: null,
                Fixture.Create<AddressPersistentLocalId>());

            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand adres.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressDeregulationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await _controller.CorrectDeregulation(
                MockIfMatchValidator(true),
                ifMatchHeaderValue: null,
                Fixture.Create<AddressPersistentLocalId>());

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
