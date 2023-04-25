namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Autofac;
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
        private readonly ILifetimeScope _lifetimeScope;

        public ReaddressLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IStreetNames streetNames,
            IIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            ILifetimeScope lifetimeScope)
            : base(
                configuration,
                retryPolicy,
                streetNames,
                ticketing,
                idempotentCommandHandler)
        {
            _backOfficeContext = backOfficeContext;
            _lifetimeScope = lifetimeScope;
        }

        protected override async Task<object> InnerHandle(ReaddressLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var addressesToReaddress = await MapReaddressAddressItems(request.Request.HerAdresseer, cancellationToken);
            var addressesToRetire = MapRetireAddressItems(request.Request.OpheffenAdressen, addressesToReaddress);

            var readdressCommand = request.ToCommand(addressesToReaddress, addressesToRetire);

            var addressesAdded = new List<(StreetNamePersistentLocalId, AddressPersistentLocalId)>();
            var addressesUpdated = new List<(StreetNamePersistentLocalId, AddressPersistentLocalId)>();

            var etagResponses = new List<ETagResponse>();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    readdressCommand.CreateCommandId(),
                    readdressCommand,
                    request.Metadata,
                    cancellationToken);

                addressesAdded = readdressCommand.ExecutionContext.AddressesAdded;
                addressesUpdated = readdressCommand.ExecutionContext.AddressesUpdated;
            }
            catch (IdempotencyException)
            {
                var streetName = await StreetNames.GetAsync(
                    new StreetNameStreamId(readdressCommand.DestinationStreetNamePersistentLocalId), cancellationToken);

                foreach (var item in request.Request.HerAdresseer)
                {
                    var destinationAddress =
                        streetName.StreetNameAddresses.FindActiveParentByHouseNumber(
                            HouseNumber.Create(item.DoelHuisnummer));
                    addressesUpdated.Add((
                        readdressCommand.DestinationStreetNamePersistentLocalId,
                        destinationAddress!.AddressPersistentLocalId));
                }

                foreach (var addressToRetirePuri in request.Request.OpheffenAdressen ?? new List<string>())
                {
                    addressesUpdated.Add((
                        readdressCommand.DestinationStreetNamePersistentLocalId,
                        new AddressPersistentLocalId(
                            Convert.ToInt32(addressToRetirePuri.AsIdentifier().Map(x => x).Value))));
                }
            }

            foreach (var addressesByStreetName in addressesToRetire
                         .Where(x => x.StreetNamePersistentLocalId != readdressCommand.DestinationStreetNamePersistentLocalId)
                         .GroupBy(x => x.StreetNamePersistentLocalId))
            {
                await using var scope = _lifetimeScope.BeginLifetimeScope();

                var streetNames = scope.Resolve<IStreetNames>();
                var streetName = await streetNames.GetAsync(
                    new StreetNameStreamId(readdressCommand.DestinationStreetNamePersistentLocalId), cancellationToken);

                foreach (var (_, addressPersistentLocalId) in addressesByStreetName)
                {
                    try
                    {
                        var houseNumber = addressesToReaddress
                            .Single(x => x.SourceAddressPersistentLocalId == addressPersistentLocalId)
                            .DestinationHouseNumber;

                        var destinationAddress = streetName.StreetNameAddresses.FindActiveParentByHouseNumber(HouseNumber.Create(houseNumber));
                        var destinationBoxNumbers = destinationAddress.Children
                            .Where(x => x.IsActive)
                            .Select(x => new BoxNumberAddressPersistentLocalId(x.BoxNumber!, x.AddressPersistentLocalId))
                            .ToList();

                        var rejectOrRetireAddresses = new RejectOrRetireAddressForReaddress(
                            addressesByStreetName.Key,
                            readdressCommand.DestinationStreetNamePersistentLocalId,
                            addressPersistentLocalId,
                            destinationAddress.AddressPersistentLocalId,
                            destinationBoxNumbers,
                            request.Provenance);

                        await scope.Resolve<IIdempotentCommandHandler>().Dispatch(
                            rejectOrRetireAddresses.CreateCommandId(),
                            rejectOrRetireAddresses,
                            request.Metadata,
                            cancellationToken);
                    }
                    catch (IdempotencyException)
                    {
                    }
                }

                addressesUpdated.AddRange(addressesByStreetName.Select(x => (addressesByStreetName.Key, x.AddressPersistentLocalId)));
            }

            foreach (var (streetNamePersistentLocalId, addressPersistentLocalId) in addressesAdded)
            {
                await _backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    addressPersistentLocalId,
                    streetNamePersistentLocalId,
                    cancellationToken);
            }

            // Only etags for house number addresses are returned as they were not part of the original request.
            // An additional argument is because in case of an IdempotencyException,
            //  we no longer know which destination box number was rejected or retired in the scope of this command or previously.
            foreach (var (streetNamePersistentLocalId, addressPersistentLocalId) in addressesUpdated.Distinct())
            {
                var lastHash = await GetHash(streetNamePersistentLocalId, addressPersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        private async Task<List<ReaddressAddressItem>> MapReaddressAddressItems(
            IEnumerable<AddressToReaddressItem> addressesToReaddress,
            CancellationToken cancellationToken)
        {
            var readdressAddressItems = new List<ReaddressAddressItem>();

            // Get streetNamePersistentLocalId for each address in the request
            foreach (var item in addressesToReaddress)
            {
                var addressPersistentLocalId =
                    new AddressPersistentLocalId(Convert.ToInt32(item.BronAdresId.AsIdentifier().Map(x => x).Value));
                var relation =
                    await _backOfficeContext.FindRelationAsync(addressPersistentLocalId, cancellationToken);

                var readdressAddressItem = new ReaddressAddressItem(
                    new StreetNamePersistentLocalId(relation.StreetNamePersistentLocalId),
                    addressPersistentLocalId,
                    HouseNumber.Create(item.DoelHuisnummer));

                readdressAddressItems.Add(readdressAddressItem);
            }

            return readdressAddressItems;
        }

        private static List<RetireAddressItem> MapRetireAddressItems(
            IEnumerable<string>? addressesToRetire,
            IEnumerable<ReaddressAddressItem> addressesToReaddress)
        {
            var retireAddressItems = new List<RetireAddressItem>();

            foreach (var item in addressesToRetire ?? new List<string>())
            {
                var addressPersistentLocalId =
                    new AddressPersistentLocalId(Convert.ToInt32(item.AsIdentifier().Map(x => x).Value));

                // We don't look into the context because OpheffenAdressen should all be in HerAdresseer
                var streetNamePersistentLocalId = addressesToReaddress
                    .Single(x => x.SourceAddressPersistentLocalId == addressPersistentLocalId)
                    .SourceStreetNamePersistentLocalId;

                retireAddressItems.Add(new RetireAddressItem(streetNamePersistentLocalId, addressPersistentLocalId));
            }

            return retireAddressItems;
        }

        protected override TicketError? InnerMapDomainException(
            DomainException exception,
            ReaddressLambdaRequest request)
        {
            return exception switch
            {
                StreetNameIsRemovedException => ValidationErrors.Common.StreetNameIsRemoved.ToTicketError(),
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameIsNotActive.ToTicketError(),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => ValidationErrors.Common.PostalCode
                    .PostalCodeNotInMunicipality.ToTicketError(),
                AddressHasInvalidStatusException ex => ValidationErrors.Readdress.AddressInvalidStatus.ToTicketError(
                    CreatePuri(ex.AddressPersistentLocalId)),
                AddressHasBoxNumberException ex => ValidationErrors.Readdress.AddressHasBoxNumber.ToTicketError(
                    CreatePuri(ex.AddressPersistentLocalId)),
                AddressHasNoPostalCodeException ex => ValidationErrors.Readdress.AddressHasNoPostalCode.ToTicketError(
                    ex.AddressPersistentLocalId),
                HouseNumberHasInvalidFormatException ex =>
                    ValidationErrors.Readdress.HouseNumberInvalidFormat.ToTicketError(ex.Message),
                SourceAndDestinationAddressAreTheSameException ex => ValidationErrors.Readdress
                    .SourceAndDestinationAddressAreTheSame.ToTicketError(CreatePuri(ex.SourceAddressPersistentLocalId)),
                _ => null
            };
        }

        private string CreatePuri(AddressPersistentLocalId addressPersistentLocalId)
        {
            return string.Format(DetailUrlFormat, addressPersistentLocalId);
        }
    }
}
