namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Validation;
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
    using StreetName.Exceptions;
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

            var addressesAdded = new List<(StreetNamePersistentLocalId, AddressPersistentLocalId)>();
            var addressesUpdated = new List<(StreetNamePersistentLocalId, AddressPersistentLocalId)>();

            var etagResponses = new List<ETagResponse>();

            try
            {
                // pass context object by reference
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);

                addressesAdded = cmd.ExecutionContext.AddressesAdded;
                addressesUpdated = cmd.ExecutionContext.AddressesAdded.Union(cmd.ExecutionContext.AddressesUpdated).ToList();
            }
            catch (IdempotencyException)
            {
                var streetName = await StreetNames.GetAsync(new StreetNameStreamId(cmd.DestinationStreetNamePersistentLocalId), cancellationToken);

                foreach (var item in request.Request.HerAdresseer)
                {
                    addressesUpdated.Add((cmd.DestinationStreetNamePersistentLocalId,  new AddressPersistentLocalId(Convert.ToInt32(item.BronAdresId.AsIdentifier().Map(x => x).Value))));

                    var doelAddress = streetName.StreetNameAddresses.FindActiveParentByHouseNumber(HouseNumber.Create(item.DoelHuisnummer));
                    addressesUpdated.Add((cmd.DestinationStreetNamePersistentLocalId, doelAddress.AddressPersistentLocalId));
                }

                foreach (var addressToRetirePuri in request.Request.OpheffenAdressen ?? new List<string>())
                {
                    addressesUpdated.Add((cmd.DestinationStreetNamePersistentLocalId,  new AddressPersistentLocalId(Convert.ToInt32(addressToRetirePuri.AsIdentifier().Map(x => x).Value))));
                }
            }

            foreach (var (streetNamePersistentLocalId, addressPersistentLocalId) in addressesAdded)
            {
                await _backOfficeContext.AddIdempotentAddressStreetNameIdRelation(addressPersistentLocalId, streetNamePersistentLocalId, cancellationToken);
            }

            foreach (var (streetNamePersistentLocalId, addressPersistentLocalId) in addressesUpdated.Distinct())
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
                StreetNameIsRemovedException => ValidationErrors.Common.StreetNameIsRemoved.ToTicketError(),
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameIsNotActive.ToTicketError(),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.ToTicketError(),
                AddressHasInvalidStatusException ex => ValidationErrors.Readdress.AddressInvalidStatus.ToTicketError(ex.AddressPersistentLocalId),
                AddressHasBoxNumberException ex => ValidationErrors.Readdress.AddressHasBoxNumber.ToTicketError(ex.AddressPersistentLocalId),
                AddressHasNoPostalCodeException ex => ValidationErrors.Readdress.AddressHasNoPostalCode.ToTicketError(ex.AddressPersistentLocalId),
                HouseNumberHasInvalidFormatException => ValidationErrors.Common.HouseNumberInvalidFormat.ToTicketError(),
                _ => null
            };
        }
    }
}
