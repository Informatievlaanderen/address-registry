namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public sealed class CorrectAddressRejectionLambdaHandler : SqsLambdaHandler<CorrectAddressRejectionLambdaRequest>
    {
        public CorrectAddressRejectionLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectAddressRejectionLambdaRequest request, CancellationToken cancellationToken)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(request.AddressPersistentLocalId);
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectAddressRejectionLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameStatusInvalidForAction.ToTicketError(),

                AddressHasInvalidStatusException => ValidationErrors.CorrectRejection.AddressInvalidStatus.ToTicketError(),

                AddressAlreadyExistsException => ValidationErrors.Common.AddressAlreadyExists.ToTicketError(),

                BoxNumberHouseNumberDoesNotMatchParentHouseNumberException =>
                    ValidationErrors.CorrectRejection.InconsistentHouseNumber.ToTicketError(),

                BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException =>
                    ValidationErrors.CorrectRejection.InconsistentPostalCode.ToTicketError(),

                ParentAddressHasInvalidStatusException => ValidationErrors.CorrectRejection.ParentInvalidStatus.ToTicketError(),

                _ => null
            };
        }
    }
}
