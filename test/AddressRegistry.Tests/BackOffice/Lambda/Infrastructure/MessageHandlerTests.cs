namespace AddressRegistry.Tests.BackOffice.Lambda.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using MediatR;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class MessageHandlerTests : BackOfficeLambdaTest
    {
        public MessageHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task WhenProcessingUnknownMessage_ThenNothingIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<object>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<TestSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            var act = async () => await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Fact]
        public async Task WhenSqsAddressApproveRequest_ThenApproveRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ApproveSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ApproveLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectApprovalRequest_ThenCorrectApprovalRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectApprovalSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectApprovalLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenChangePositionRequest_ThenChangePositionRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ChangePositionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ChangePositionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenChangePostalCodeRequest_ThenChangePostalCodeRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ChangePostalCodeSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ChangePostalCodeLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectHouseNumberRequest_ThenCorrectHouseNumberRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectHouseNumberSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectHouseNumberLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectPositionRequest_ThenCorrectPositionRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectPositionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectPositionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectPostalCodeRequest_ThenCorrectPostalCodeRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectPostalCodeSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectPostalCodeLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenSqsAddressDeregulateRequest_ThenDeregulateRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<DeregulateSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<DeregulateLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProposeRequest_ThenProposeRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var expectedRequest = Fixture.Create<ProposeSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                expectedRequest,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ProposeLambdaRequest>(actualRequest =>
                    actualRequest.TicketId == expectedRequest.TicketId
                    && actualRequest.MessageGroupId == messageMetadata.MessageGroupId
                    && actualRequest.Request == expectedRequest.Request
                    && actualRequest.IfMatchHeaderValue == expectedRequest.IfMatchHeaderValue
                    && actualRequest.Provenance == expectedRequest.ProvenanceData.ToProvenance()
                    && actualRequest.Metadata == expectedRequest.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenRegularizeRequest_ThenRegularizeRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RegularizeSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RegularizeLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenRejectRequest_ThenRejectLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RejectSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RejectLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenRemoveRequest_ThenRemoveLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RemoveSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RemoveLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenRetireRequest_ThenRetireLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RetireSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RetireLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectRejectionRequest_ThenCorrectRejectionRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectRejectionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectRejectionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectRetirementRequest_ThenCorrectRetirementRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectRetirementSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectRetirementLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectBoxNumberRequest_ThenCorrectBoxNumberRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBoxNumberSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBoxNumberLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }
    }

    public class TestSqsRequest : SqsRequest
    { }
}
