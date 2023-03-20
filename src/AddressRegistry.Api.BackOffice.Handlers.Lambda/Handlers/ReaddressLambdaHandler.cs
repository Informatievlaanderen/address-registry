namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Commands;
    using TicketingService.Abstractions;

    public sealed class ReaddressLambdaHandler : SqsLambdaHandler<ReaddressLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public ReaddressLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IStreetNames streetNames,
            IIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext)
            : base(
                configuration,
                retryPolicy,
                streetNames,
                ticketing,
                idempotentCommandHandler)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<object> InnerHandle(ReaddressLambdaRequest request, CancellationToken cancellationToken)
        {
            // Get streetNamePersistentLocalId for each address in the request
            // TODO: Describe steps being executed

            var readdressAddressItems = new List<ReaddressAddressItem>();

            foreach (var item in request.Request.HerAdresseer)
            {
                var addressPersistentLocalId =
                    new AddressPersistentLocalId(Convert.ToInt32(item.BronAdresId.AsIdentifier().Map(x => x).Value));
                var relation = await _backOfficeContext.FindRelationAsync(addressPersistentLocalId, cancellationToken);

                var readdressAddressItem = new ReaddressAddressItem(
                    new StreetNamePersistentLocalId(relation.StreetNamePersistentLocalId),
                    addressPersistentLocalId,
                    HouseNumber.Create(item.DoelHuisnummer));

                readdressAddressItems.Add(readdressAddressItem);
            }

            var retireAddressItems = new List<RetireAddressItem>();
            
            foreach (var item in request.Request.OpheffenAdressen ?? new List<string>())
            {
                var addressPersistentLocalId =
                    new AddressPersistentLocalId(Convert.ToInt32(item.AsIdentifier().Map(x => x).Value));

                // We don't look into the context because OpheffenAdressen should all be in HerAdresseer
                var streetNamePersistentLocalId = readdressAddressItems
                        .Single(x => x.SourceAddressPersistentLocalId == addressPersistentLocalId)
                        .SourceStreetNamePersistentLocalId;

                retireAddressItems.Add(new RetireAddressItem(streetNamePersistentLocalId, addressPersistentLocalId));
            }

            var cmd = request.ToCommand(readdressAddressItems, retireAddressItems);

            try
            {
                // pass context object by reference
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

            var etagResponses = new List<ETagResponse>();
            
            // add relations between new addresses & destinationStreetName
            foreach (var (streetNamePersistentLocalId, addressPersistentLocalId) in cmd.ExecutionContext.AddressesAdded)
            {
                await _backOfficeContext.AddIdempotentAddressStreetNameIdRelation(addressPersistentLocalId, streetNamePersistentLocalId, cancellationToken);
            }

            foreach (var (streetNamePersistentLocalId, addressPersistentLocalId) in cmd.ExecutionContext.AddressesAdded.Union(cmd.ExecutionContext.AddressesUpdated))
            {
                var lastHash = await GetHash(streetNamePersistentLocalId, addressPersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ReaddressLambdaRequest request)
        {
            return exception switch
            {
                // StreetNameHasInvalidStatusException => new TicketError(
                //     ValidationErrors.Common.StreetNameIsNotActive.Message,
                //     ValidationErrors.Common.StreetNameIsNotActive.Code),
                // AddressHasInvalidStatusException => new TicketError(
                //     ValidationErrors.RetireAddress.AddressInvalidStatus.Message,
                //     ValidationErrors.RetireAddress.AddressInvalidStatus.Code),
                _ => null
            };
        }
    }
}
