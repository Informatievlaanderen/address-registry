namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using TicketingService.Abstractions;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;
    using static Microsoft.AspNetCore.Http.Results;

    public abstract class SqsHandler<TRequest> : IRequestHandler<TRequest, IResult>
        where TRequest : SqsRequest
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketing _ticketing;
        private readonly ITicketingUrl _ticketingUrl;

        protected SqsHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
        {
            _sqsOptions = sqsOptions;
            _ticketing = ticketing;
            _ticketingUrl = ticketingUrl;
        }

        protected abstract string WithGroupId(TRequest request);

        public async Task<IResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var ticketId = await _ticketing.CreateTicket(nameof(AddressRegistry), cancellationToken);
            request.TicketId = ticketId;

            var groupId = WithGroupId(request);
            request.MessageGroupId = groupId;

            if (string.IsNullOrEmpty(groupId))
            {
                throw new InvalidOperationException("No groupId.");
            }

            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, new SqsQueueOptions { MessageGroupId = request.MessageGroupId }, cancellationToken);

            //_logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            var location = _ticketingUrl.For(request.TicketId);
            return Accepted(location);
        }
    }
}
