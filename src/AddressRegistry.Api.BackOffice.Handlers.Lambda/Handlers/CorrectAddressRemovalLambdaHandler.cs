namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public sealed class CorrectAddressRemovalLambdaHandler : SqsLambdaHandler<CorrectAddressRemovalLambdaRequest>
    {
        public CorrectAddressRemovalLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectAddressRemovalLambdaRequest request, CancellationToken cancellationToken)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(request.Request.PersistentLocalId);
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

            var lastHash = await GetHash(request.StreetNamePersistentLocalId(), addressPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectAddressRemovalLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameStatusInvalidForAction.ToTicketError(),

                ParentAddressIsRemovedException => ValidationErrors.Common.ParentAddressRemoved.ToTicketError(),

                ParentAddressHasInvalidStatusException => ValidationErrors.CorrectRemoval.ParentInvalidStatus.ToTicketError(),
                
                BoxNumberHouseNumberDoesNotMatchParentHouseNumberException =>
                    ValidationErrors.Common.AddressInconsistentHouseNumber.ToTicketError(),

                BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException =>
                    ValidationErrors.Common.AddressInconsistentPostalCode.ToTicketError(),

                AddressAlreadyExistsException => ValidationErrors.Common.AddressAlreadyExists.ToTicketError(),

                _ => null
            };
        }
    }
}
