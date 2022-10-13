namespace AddressRegistry.Tests.BackOffice.Sqs
{
    using AddressRegistry.Api.BackOffice.Handlers.Sqs;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenAddressBackOfficeProposeRequest : AddressRegistryTest
    {
        public GivenAddressBackOfficeProposeRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var streetNameId = 45041;

            var request = Fixture.Create<AddressBackOfficeProposeRequest>();
            request.StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNameId}";
            var sqsRequest = new SqsAddressProposeRequest { Request = request };

            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new SqsAddressProposeHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl);

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);
            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == streetNameId.ToString()),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }
    }
}
