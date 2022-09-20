namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Responses;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;

    public sealed class SqsAddressApproveHandler : SqsLambdaHandler<SqsLambdaAddressApproveRequest>
    {
        public SqsAddressApproveHandler(
             IConfiguration configuration,
             ICustomRetryPolicy retryPolicy,
             ITicketing ticketing,
             IStreetNames streetNames,
             IIdempotentCommandHandler idempotentCommandHandler)
             : base(
                 configuration,
                 retryPolicy,
                 streetNames,
                 ticketing,
                 idempotentCommandHandler)
        { }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressApproveRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new AddressPersistentLocalId(request.Request.PersistentLocalId);
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetHash(request.StreetNamePersistentLocalId, streetNamePersistentLocalId, cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressApproveRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameIsNotActive,
                    ValidationErrors.StreetName.StreetNameIsNotActive),
                StreetNameIsRemovedException => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameInvalid(request.StreetNamePersistentLocalId),
                    ValidationErrors.StreetName.StreetNameInvalid),

                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressCannotBeApproved,
                    ValidationErrors.Address.AddressCannotBeApproved),
                ParentAddressHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressCannotBeApprovedBecauseOfParent,
                    ValidationErrors.Address.AddressCannotBeApprovedBecauseOfParent),
                _ => null
            };
        }
    }
}
