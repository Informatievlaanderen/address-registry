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
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
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
        public async Task WhenSqsApproveAddressRequest_ThenApproveRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ApproveAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ApproveAddressLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressApprovalSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressApprovalLambdaRequest>(request =>
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

            var messageData = Fixture.Create<ChangeAddressPositionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ChangeAddressPositionLambdaRequest>(request =>
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

            var messageData = Fixture.Create<ChangeAddressPostalCodeSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ChangeAddressPostalCodeLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressHouseNumberSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressHouseNumberLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressPositionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressPositionLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressPostalCodeSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressPostalCodeLambdaRequest>(request =>
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

            var messageData = Fixture.Create<DeregulateAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<DeregulateAddressLambdaRequest>(request =>
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

            var expectedRequest = Fixture.Create<ProposeAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                expectedRequest,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ProposeAddressLambdaRequest>(actualRequest =>
                    actualRequest.TicketId == expectedRequest.TicketId
                    && actualRequest.MessageGroupId == messageMetadata.MessageGroupId
                    && actualRequest.Request == expectedRequest.Request
                    && actualRequest.IfMatchHeaderValue == null
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

            var messageData = Fixture.Create<RegularizeAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RegularizeAddressLambdaRequest>(request =>
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

            var messageData = Fixture.Create<RejectAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RejectAddressLambdaRequest>(request =>
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

            var messageData = Fixture.Create<RemoveAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RemoveAddressLambdaRequest>(request =>
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

            var messageData = Fixture.Create<RetireAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RetireAddressLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressRejectionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressRejectionLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressRetirementSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressRetirementLambdaRequest>(request =>
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

            var messageData = Fixture.Create<CorrectAddressBoxNumberSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressBoxNumberLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.AddressPersistentLocalId == messageData.PersistentLocalId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenCorrectAddressRegularizationRequest_ThenSend()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectAddressRegularizationSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectAddressRegularizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.AddressPersistentLocalId == messageData.Request.PersistentLocalId
                    && request.Request == messageData.Request
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }


        [Fact]
        public async Task WhenCorrectAddressDeregulationRequest_ThenSendLambdaRequest()
        {
            await AssertSqsRequestWithLambdaRequest<CorrectAddressDeregulationSqsRequest, CorrectAddressDeregulationLambdaRequest>(
                (sqsRequest, lambdaRequest)
                    => lambdaRequest.AddressPersistentLocalId == sqsRequest.Request.PersistentLocalId);
        }

        public async Task AssertSqsRequestWithLambdaRequest<TSqsRequest, TLambdaRequest>(Func<TSqsRequest, TLambdaRequest, bool> customAssertions)
            where TSqsRequest : SqsRequest
            where TLambdaRequest: SqsLambdaRequest
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<TSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<TLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId
                    && request.MessageGroupId == messageMetadata.MessageGroupId
                    && request.IfMatchHeaderValue == messageData.IfMatchHeaderValue
                    && request.Provenance == messageData.ProvenanceData.ToProvenance()
                    && request.Metadata == messageData.Metadata
                    && customAssertions(messageData, request)
                ), CancellationToken.None), Times.Once);
        }
    }

    public class TestSqsRequest : SqsRequest
    { }
}
