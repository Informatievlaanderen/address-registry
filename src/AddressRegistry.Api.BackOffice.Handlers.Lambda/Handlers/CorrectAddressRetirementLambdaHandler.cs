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

    public sealed class CorrectAddressRetirementLambdaHandler : SqsLambdaHandler<CorrectAddressRetirementLambdaRequest>
    {
        public CorrectAddressRetirementLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectAddressRetirementLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectAddressRetirementLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameStatusInvalidForAction.ToTicketError(),

                AddressHasInvalidStatusException => ValidationErrors.CorrectRetirement.AddressInvalidStatus.ToTicketError(),

                AddressAlreadyExistsException => ValidationErrors.Common.AddressAlreadyExists.ToTicketError(),

                BoxNumberHouseNumberDoesNotMatchParentHouseNumberException =>
                    ValidationErrors.CorrectRetirement.InconsistentHouseNumber.ToTicketError(),

                BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException =>
                    ValidationErrors.CorrectRetirement.InconsistentPostalCode.ToTicketError(),

                ParentAddressHasInvalidStatusException => ValidationErrors.CorrectRetirement.ParentInvalidStatus.ToTicketError(),

                _ => null
            };
        }
    }
}
