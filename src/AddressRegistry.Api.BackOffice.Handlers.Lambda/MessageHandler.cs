namespace AddressRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.SqsRequests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using MediatR;
    using Requests;

    public sealed class MessageHandler : IMessageHandler
    {
        private readonly ILifetimeScope _container;

        public MessageHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken _)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            if (messageData is not SqsRequest sqsRequest)
            {
                messageMetadata.Logger?.LogInformation($"Unable to cast {nameof(messageData)} as {nameof(sqsRequest)}.");
                return;
            }

            await using var lifetimeScope = _container.BeginLifetimeScope();
            var mediator = lifetimeScope.Resolve<IMediator>();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(5));
            var cancellationToken = cancellationTokenSource.Token;

            switch (sqsRequest)
            {
                case ApproveAddressSqsRequest request:
                    await mediator.Send(new ApproveAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressApprovalSqsRequest request:
                    await mediator.Send(new CorrectAddressApprovalLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ChangeAddressPositionSqsRequest request:
                    await mediator.Send(new ChangeAddressPositionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ChangeAddressPostalCodeSqsRequest request:
                    await mediator.Send(new ChangeAddressPostalCodeLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressHouseNumberSqsRequest request:
                    await mediator.Send(new CorrectAddressHouseNumberLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressPositionSqsRequest request:
                    await mediator.Send(new CorrectAddressPositionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressPostalCodeSqsRequest request:
                    await mediator.Send(new CorrectAddressPostalCodeLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressRejectionSqsRequest request:
                    await mediator.Send(new CorrectAddressRejectionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressRemovalSqsRequest request:
                    await mediator.Send(new CorrectAddressRemovalLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case DeregulateAddressSqsRequest request:
                    await mediator.Send(new DeregulateAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ProposeAddressSqsRequest request:
                    await mediator.Send(new ProposeAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ProposeAddressesForMunicipalityMergerSqsRequest request:
                    await mediator.Send(new ProposeAddressesForMunicipalityMergerLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RegularizeAddressSqsRequest request:
                    await mediator.Send(new RegularizeAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RejectAddressSqsRequest request:
                    await mediator.Send(new RejectAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RemoveAddressSqsRequest request:
                    await mediator.Send(new RemoveAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case RetireAddressSqsRequest request:
                    await mediator.Send(new RetireAddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressRetirementSqsRequest request:
                    await mediator.Send(new CorrectAddressRetirementLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressBoxNumberSqsRequest request:
                    await mediator.Send(new CorrectAddressBoxNumberLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressRegularizationSqsRequest request:
                    await mediator.Send(new CorrectAddressRegularizationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectAddressDeregulationSqsRequest request:
                    await mediator.Send(new CorrectAddressDeregulationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case ReaddressSqsRequest request:
                    await mediator.Send(new ReaddressLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CreateStreetNameSnapshotSqsRequest request:
                    await mediator.Send(new CreateStreetNameSnapshotLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                default:
                    throw new NotImplementedException(
                        $"{sqsRequest.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
