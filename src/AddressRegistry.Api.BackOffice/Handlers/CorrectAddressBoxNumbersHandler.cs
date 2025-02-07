namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class CorrectAddressBoxNumbersHandler : SqsHandler<CorrectAddressBoxNumbersSqsRequest>
    {
        public const string Action = "CorrectAddressBoxNumbers";

        private readonly BackOfficeContext _backOfficeContext;

        public CorrectAddressBoxNumbersHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(CorrectAddressBoxNumbersSqsRequest request)
        {
            var addressPersistentLocalId = request.Request.Busnummers.First().AdresId.AsIdentifier().Map(int.Parse);

            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(addressPersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectAddressBoxNumbersSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(AddressRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId }
            };
        }
    }
}
