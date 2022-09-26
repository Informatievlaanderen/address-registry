namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Responses;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public abstract class SqsLambdaHandler<TSqsLambdaRequest> : IRequestHandler<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaRequest
    {
        private readonly ITicketing _ticketing;
        private readonly ICustomRetryPolicy _retryPolicy;
        private readonly IStreetNames _streetNames;

        protected IIdempotentCommandHandler IdempotentCommandHandler { get; }
        protected string DetailUrlFormat { get; }

        protected SqsLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IStreetNames streetNames,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
        {
            _retryPolicy = retryPolicy;
            _streetNames = streetNames;
            _ticketing = ticketing;
            IdempotentCommandHandler = idempotentCommandHandler;

            DetailUrlFormat = configuration["DetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new NullReferenceException("'DetailUrl' cannot be found in the configuration");
            }
        }

        protected abstract Task<ETagResponse> InnerHandle(TSqsLambdaRequest request, CancellationToken cancellationToken);

        protected abstract TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request);

        public async Task<Unit> Handle(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateIfMatchHeaderValue(request, cancellationToken);

                await _ticketing.Pending(request.TicketId, cancellationToken);

                ETagResponse? etag = null;

                await _retryPolicy.Retry(async () => etag = await InnerHandle(request, cancellationToken));

                await _ticketing.Complete(
                    request.TicketId,
                    new TicketResult(etag),
                    cancellationToken);
            }
            catch (IfMatchHeaderValueMismatchException)
            {
                await _ticketing.Error(
                    request.TicketId,
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                    cancellationToken);
            }
            catch (DomainException exception)
            {
                var ticketError = exception switch
                {
                    AddressIsNotFoundException => new TicketError(
                        ValidationErrorMessages.Address.AddressNotFound,
                        ValidationErrors.Address.AddressNotFound),
                    AddressIsRemovedException => new TicketError(
                        ValidationErrorMessages.Address.AddressRemoved,
                        ValidationErrors.Address.AddressRemoved),
                    _ => MapDomainException(exception, request)
                };

                ticketError ??= new TicketError(exception.Message, "");

                await _ticketing.Error(
                    request.TicketId,
                    ticketError,
                    cancellationToken);
            }

            return Unit.Value;
        }

        private async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not Abstractions.IHasAddressPersistentLocalId id)
                return;

            var lastHash = await GetHash(
                request.StreetNamePersistentLocalId,
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
    }
}
