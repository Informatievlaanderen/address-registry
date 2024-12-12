namespace AddressRegistry.Tests.BackOffice.Api.WhenRequestingCreateStreetNameSnapshot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<CreateStreetNameSnapshotSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<CreateStreetNameSnapshotRequest>();

            var result = (AcceptedResult)await _controller.CreateSnapshot(request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CreateStreetNameSnapshotSqsRequest>(sqsRequest =>
                            sqsRequest.Request == request
                            && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                            && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                            && sqsRequest.ProvenanceData.Modification == Modification.Unknown
                    ),
                    CancellationToken.None));
        }
    }
}
