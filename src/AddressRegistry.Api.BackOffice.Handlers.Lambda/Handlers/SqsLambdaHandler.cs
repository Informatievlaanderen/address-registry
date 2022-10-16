namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using TicketingService.Abstractions;

    public abstract class SqsLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaRequest
    {
        private readonly IStreetNames _streetNames;

        protected SqsLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IStreetNames streetNames,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(configuration, retryPolicy, ticketing, idempotentCommandHandler)
        {
            _streetNames = streetNames;
        }

        protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not Abstractions.IHasAddressPersistentLocalId id)
            {
                return;
            }

            var lastHash = await GetHash(
                request.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(id.AddressPersistentLocalId),
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            if (request.IfMatchHeaderValue != lastHashTag.ToString())
            {
                throw new IfMatchHeaderValueMismatchException();
            }
        }

        protected async Task<string> GetHash(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _streetNames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), cancellationToken);
            var streetNameHash = aggregate.GetAddressHash(addressPersistentLocalId);
            return streetNameHash;
        }
        
        protected override async Task HandleException<T>(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            await Ticketing.Error(request.TicketId,
                new TicketError(ValidationErrors.Common.AddressNotFound.Message, "404"),
                cancellationToken);
        }
    }
}
