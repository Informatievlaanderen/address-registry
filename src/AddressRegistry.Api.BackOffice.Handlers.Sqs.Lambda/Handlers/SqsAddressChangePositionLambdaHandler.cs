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

            var lastHash = await GetHash(request.StreetNamePersistentLocalId, addressPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressChangePositionRequest request)
        {
            return exception switch
            {
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressPositionCannotBeChanged,
                    ValidationErrors.Address.AddressPositionCannotBeChanged),
                AddressHasInvalidGeometryMethodException => new TicketError(
                    ValidationErrorMessages.Address.GeometryMethodInvalid,
                    ValidationErrors.Address.GeometryMethodInvalid),
                AddressHasMissingGeometrySpecificationException => new TicketError(
                    ValidationErrorMessages.Address.PositionSpecificationRequired,
                    ValidationErrors.Address.PositionSpecificationRequired),
                AddressHasInvalidGeometrySpecificationException => new TicketError(
                    ValidationErrorMessages.Address.PositionSpecificationInvalid,
                    ValidationErrors.Address.PositionSpecificationInvalid),
                _ => null
            };
        }
    }
}
