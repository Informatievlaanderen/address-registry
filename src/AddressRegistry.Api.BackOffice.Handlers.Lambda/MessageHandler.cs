namespace AddressRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using MediatR;
    using Requests;
    using Sqs.Requests;

    public sealed class MessageHandler : IMessageHandler
    {
        private readonly ILifetimeScope _container;

        public MessageHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            if (messageData is not SqsRequest sqsRequest)
            {
                messageMetadata.Logger?.LogInformation($"Unable to cast {nameof(messageData)} as {nameof(sqsRequest)}.");
                return;
            }

            await using var lifetimeScope = _container.BeginLifetimeScope();
            var mediator = lifetimeScope.Resolve<IMediator>();

            switch (sqsRequest)
            {
                case ApproveSqsRequest request:
                    await mediator.Send(new ApproveLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectApprovalSqsRequest request:
                    await mediator.Send(new CorrectApprovalLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ChangePositionSqsRequest request:
                    await mediator.Send(new ChangePositionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ChangePostalCodeSqsRequest request:
                    await mediator.Send(new ChangePostalCodeLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectHouseNumberSqsRequest request:
                    await mediator.Send(new CorrectHouseNumberLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectPositionSqsRequest request:
                    await mediator.Send(new CorrectPositionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectPostalCodeSqsRequest request:
                    await mediator.Send(new CorrectPostalCodeLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectRejectionSqsRequest request:
                    await mediator.Send(new CorrectRejectionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case DeregulateSqsRequest request:
                    await mediator.Send(new DeregulateLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ProposeSqsRequest request:
                    await mediator.Send(new ProposeLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RegularizeSqsRequest request:
                    await mediator.Send(new RegularizeLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RejectSqsRequest request:
                    await mediator.Send(new RejectLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RemoveSqsRequest request:
                    await mediator.Send(new RemoveLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RetireSqsRequest request:
                    await mediator.Send(new RetireLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectRetirementSqsRequest request:
                    await mediator.Send(new CorrectRetirementLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectBoxNumberSqsRequest request:
                    await mediator.Send(new CorrectBoxNumberLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                default:
                    throw new NotImplementedException(
                        $"{sqsRequest.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
