namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenProposeAddressRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenProposeAddressRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<ProposeAddressSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));
            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(persistentLocalId);

            var request = Fixture.Create<ProposeAddressRequest>();
            request.Positie = GeometryHelpers.GmlPointGeometry;

            var result = (AcceptedResult)await _controller.Propose(
                MockValidRequestValidator<ProposeAddressRequest>(),
                new ProposeAddressSqsRequestFactory(persistentLocalIdGenerator.Object),
                new GmlPositionNormalizer(new UseLambert2008EventStoreToggle(false)),
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeAddressSqsRequest>(sqsRequest =>
                        sqsRequest.PersistentLocalId == persistentLocalId
                        && sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));
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
                .Setup(x => x.Send(It.IsAny<ProposeAddressSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new LocationResult(CreateTicketUri(Fixture.Create<Guid>()))));

            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(Fixture.Create<PersistentLocalId>());

            var request = Fixture.Create<ProposeAddressRequest>();
            request.Positie = requestedPosition;

            await _controller.Propose(
                MockValidRequestValidator<ProposeAddressRequest>(),
                new ProposeAddressSqsRequestFactory(persistentLocalIdGenerator.Object),
                new GmlPositionNormalizer(new UseLambert2008EventStoreToggle(useLambert2008EventStore)),
                request);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeAddressSqsRequest>(sqsRequest => sqsRequest.Request.Positie == expectedPosition),
                    CancellationToken.None));
        }
    }
}
