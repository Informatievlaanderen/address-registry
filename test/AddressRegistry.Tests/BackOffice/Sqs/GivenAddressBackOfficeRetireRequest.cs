namespace AddressRegistry.Tests.BackOffice.Sqs
{
    using AddressRegistry.Api.BackOffice.Abstractions.Exceptions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using StreetName;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenAddressBackOfficeRetireRequest : AddressRegistryTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressBackOfficeRetireRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

            var sut = new SqsAddressRetireHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl,
                _backOfficeContext);

            var sqsRequest = new SqsAddressRetireRequest
            {
                Request = new AddressBackOfficeRetireRequest
                {
                    PersistentLocalId = Fixture.Create<AddressPersistentLocalId>()
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
            var sut = new SqsAddressRetireHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _backOfficeContext);

            // Act
            var act = async () => await sut.Handle(
                new SqsAddressRetireRequest
                {
                    Request = Fixture.Create<AddressBackOfficeRetireRequest>()
                }, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
