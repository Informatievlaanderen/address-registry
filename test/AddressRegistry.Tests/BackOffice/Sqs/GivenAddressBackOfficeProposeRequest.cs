namespace AddressRegistry.Tests.BackOffice.Sqs
{
    using AddressRegistry.Api.BackOffice.Abstractions.Exceptions;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs;
    using BackOffice;
    using Infrastructure;
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
    using Consumer.Read.Municipality.Projections;
    using StreetName;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenAddressBackOfficeProposeRequest : AddressRegistryBackOfficeTest
    {
        private readonly TestMunicipalityConsumerContext _municipalityConsumerContext;

        public GivenAddressBackOfficeProposeRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _municipalityConsumerContext = new FakeMunicipalityConsumerContextFactory().CreateDbContext();
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var nisCode = new NisCode("1234");

            _municipalityConsumerContext.MunicipalityLatestItems.Add(new MunicipalityLatestItem
            {
                MunicipalityId = Fixture.Create<MunicipalityId>(),
                NisCode = nisCode
            });

            await _municipalityConsumerContext.SaveChangesAsync();

            var request = Fixture.Create<AddressBackOfficeProposeRequest>();
            request.PostInfoId = $"https://data.vlaanderen.be/id/gemeente/{nisCode}";
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
                ticketingUrl,
                _municipalityConsumerContext);

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);
            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == ((Guid) Fixture.Create<MunicipalityId>()).ToString("D")),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }

        [Fact]
        public void ForNotExistingMunicipality_ThrowsAggregateIdNotFound()
        {
            // Arrange
            var sut = new SqsAddressProposeHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _municipalityConsumerContext);

            // Act
            var act = async () => await sut.Handle(
                new SqsAddressProposeRequest { Request = Fixture.Create<AddressBackOfficeProposeRequest>() },
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
