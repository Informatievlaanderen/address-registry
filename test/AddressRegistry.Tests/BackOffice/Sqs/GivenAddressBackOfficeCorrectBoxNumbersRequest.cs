namespace AddressRegistry.Tests.BackOffice.Sqs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using StreetName;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenAddressBackOfficeCorrectBoxNumbersRequest : AddressRegistryTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressBackOfficeCorrectBoxNumbersRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedAddressPersistentLocalId());

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var addAddressPersistentIdStreetNamePersistentId = await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<StreetNamePersistentLocalId>());

            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new CorrectAddressBoxNumbersHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl,
                _backOfficeContext);

            var sqsRequest = new CorrectAddressBoxNumbersSqsRequest
            {
                Request = new CorrectAddressBoxNumbersRequest
                {
                    Busnummers =
                    [
                        new()
                        {
                            AdresId = $"http://a/{addAddressPersistentIdStreetNamePersistentId.AddressPersistentLocalId}",
                            Busnummer = "1A"
                        }
                    ]
                }
            };

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);
            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == addAddressPersistentIdStreetNamePersistentId.StreetNamePersistentLocalId.ToString("D")),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }

        [Fact]
        public void WithNoStreetNameFoundByAddressPersistentLocalId_ThrowsAggregateIdNotFound()
        {
            // Arrange
            var sut = new CorrectAddressBoxNumbersHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _backOfficeContext);

            // Act
            var act = async () => await sut.Handle(
                new CorrectAddressBoxNumbersSqsRequest
                {
                    Request = Fixture.Create<CorrectAddressBoxNumbersRequest>()
                }, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
