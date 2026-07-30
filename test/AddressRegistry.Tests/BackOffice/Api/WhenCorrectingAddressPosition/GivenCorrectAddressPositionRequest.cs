namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressPosition
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Infrastructure;
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

    public class GivenCorrectAddressPositionRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenCorrectAddressPositionRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        /// <summary>
        /// The position normalizer runs on a request that has already passed validation,
        /// so it always gets a valid GML point.
        /// </summary>
        private CorrectAddressPositionRequest CreateRequest(string positie = GeometryHelpers.GmlPointGeometry)
        {
            var request = Fixture.Create<CorrectAddressPositionRequest>();
            request.Positie = positie;
            return request;
        }

        private static GmlPositionNormalizer Normalizer(bool useLambert2008EventStore = false)
            => new GmlPositionNormalizer(new UseLambert2008EventStoreToggle(useLambert2008EventStore));

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            var expectedIfMatchHeader = Fixture.Create<string>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressPositionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = CreateRequest();

            var result = (AcceptedResult)await _controller.CorrectPosition(
                MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(true),
                Normalizer(),
                Fixture.Create<AddressPersistentLocalId>(),
                request,
                ifMatchHeaderValue: expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectAddressPositionSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
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
            var result = await _controller.CorrectPosition(
                MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(false),
                Normalizer(),
                Fixture.Create<AddressPersistentLocalId>(),
                CreateRequest(),
                ifMatchHeaderValue: null);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WithAggregateNotFoundException_ThenThrowsApiException()
        {
            Func<Task> act = async () => await _controller.CorrectPosition(
                MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidatorThrowsAggregateNotFoundException(),
                Normalizer(),
                Fixture.Create<AddressPersistentLocalId>(),
                CreateRequest(),
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
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressPositionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await _controller.CorrectPosition(MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(true),
                Normalizer(),
                Fixture.Create<AddressPersistentLocalId>(),
                CreateRequest(),
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

        [Theory]
        [InlineData(false, GeometryHelpers.GmlPointGeometry, GeometryHelpers.GmlPointGeometry)]
        [InlineData(false, GeometryHelpers.GmlPointGeometryLambert2008, GeometryHelpers.NormalizedGmlPointGeometry)]
        [InlineData(true, GeometryHelpers.GmlPointGeometry, GeometryHelpers.NormalizedGmlPointGeometryLambert2008)]
        [InlineData(true, GeometryHelpers.GmlPointGeometryLambert2008, GeometryHelpers.GmlPointGeometryLambert2008)]
        public async Task ThenPositionIsSentInTheEventStoreReferenceSystem(
            bool useLambert2008EventStore,
            string requestedPosition,
            string expectedPosition)
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressPositionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new LocationResult(CreateTicketUri(Fixture.Create<Guid>()))));

            await _controller.CorrectPosition(
                MockValidRequestValidator<CorrectAddressPositionRequest>(),
                MockIfMatchValidator(true),
                Normalizer(useLambert2008EventStore),
                Fixture.Create<AddressPersistentLocalId>(),
                CreateRequest(requestedPosition),
                ifMatchHeaderValue: null);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CorrectAddressPositionSqsRequest>(sqsRequest => sqsRequest.Request.Positie == expectedPosition),
                    CancellationToken.None));
        }
    }
}
