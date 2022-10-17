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

    public sealed class SqsAddressChangePositionLambdaHandler : SqsLambdaHandler<SqsLambdaAddressChangePositionRequest>
    {
        public SqsAddressChangePositionLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressChangePositionRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, SqsLambdaAddressChangePositionRequest request)
        {
            return exception switch
            {
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.Position.CannotBeChanged.Message,
                    ValidationErrors.Common.Position.CannotBeChanged.Code),
                AddressHasInvalidGeometryMethodException => new TicketError(
                    ValidationErrors.Common.PositionGeometryMethod.Invalid.Message,
                    ValidationErrors.Common.PositionGeometryMethod.Invalid.Code),
                AddressHasMissingGeometrySpecificationException => new TicketError(
                    ValidationErrors.Common.PositionSpecification.Required.Message,
                    ValidationErrors.Common.PositionSpecification.Required.Code),
                AddressHasInvalidGeometrySpecificationException => new TicketError(
                    ValidationErrors.Common.PositionSpecification.Invalid.Message,
                    ValidationErrors.Common.PositionSpecification.Invalid.Code),
                _ => null
            };
        }
    }
}
