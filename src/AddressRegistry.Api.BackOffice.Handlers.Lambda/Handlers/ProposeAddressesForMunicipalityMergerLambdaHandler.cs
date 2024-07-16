namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public sealed class ProposeAddressesForMunicipalityMergerLambdaHandler : SqsLambdaHandler<ProposeAddressesForMunicipalityMergerLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public ProposeAddressesForMunicipalityMergerLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            IStreetNames streetNames)
            : base(
                configuration,
                retryPolicy,
                streetNames,
                ticketing,
                idempotentCommandHandler)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<object> InnerHandle(ProposeAddressesForMunicipalityMergerLambdaRequest request, CancellationToken cancellationToken)
        {
            var command = await BuildCommand(request, cancellationToken);

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    command.CreateCommandId(),
                    command,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var etagResponses = new List<ETagResponse>();

            foreach (var address in command.Addresses)
            {
                await _backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    address.AddressPersistentLocalId, request.StreetNamePersistentLocalId(), cancellationToken);

                var lastHash = await GetHash(request.StreetNamePersistentLocalId(), address.AddressPersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, address.AddressPersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        private async Task<ProposeAddressesForMunicipalityMerger> BuildCommand(
            ProposeAddressesForMunicipalityMergerLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var addresses = await Task.WhenAll(request.Addresses
                .Select(async x =>
                {
                    var streetName = await StreetNames.GetAsync(
                        new StreetNameStreamId(new StreetNamePersistentLocalId(x.MergedStreetNamePersistentLocalId)), cancellationToken);

                    var address = streetName.StreetNameAddresses.GetByPersistentLocalId(
                        new AddressPersistentLocalId(x.MergedAddressPersistentLocalId));

                    return new ProposeAddressesForMunicipalityMergerItem(
                        new PostalCode(x.PostalCode),
                        new AddressPersistentLocalId(x.NewAddressPersistentLocalId),
                        HouseNumber.Create(x.HouseNumber),
                        x.BoxNumber is not null ? new BoxNumber(x.BoxNumber) : null,
                        address.Geometry.GeometryMethod,
                        address.Geometry.GeometrySpecification,
                        address.Geometry.Geometry,
                        address.IsOfficiallyAssigned,
                        new AddressPersistentLocalId(x.MergedAddressPersistentLocalId));
                })
                .ToList());

            return new ProposeAddressesForMunicipalityMerger(
                new StreetNamePersistentLocalId(request.StreetNamePersistentLocalId()),
                addresses,
                request.Provenance);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ProposeAddressesForMunicipalityMergerLambdaRequest request)
        {
            return exception switch
            {
                ParentAddressAlreadyExistsException => new TicketError(
                    ValidationErrors.Common.AddressAlreadyExists.Message,
                    ValidationErrors.Common.AddressAlreadyExists.Code),
                HouseNumberHasInvalidFormatException => new TicketError(
                    ValidationErrors.Common.HouseNumberInvalidFormat.Message,
                    ValidationErrors.Common.HouseNumberInvalidFormat.Code),
                AddressAlreadyExistsException => new TicketError(
                    ValidationErrors.Common.AddressAlreadyExists.Message,
                    ValidationErrors.Common.AddressAlreadyExists.Code),
                ParentAddressNotFoundException e => new TicketError(
                    ValidationErrors.Propose.AddressHouseNumberUnknown.Message(e.StreetNamePersistentLocalId, e.HouseNumber),
                    ValidationErrors.Propose.AddressHouseNumberUnknown.Code),
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.StreetNameIsNotActive.Message,
                    ValidationErrors.Common.StreetNameIsNotActive.Code),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => new TicketError(
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Message,
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Code),
                AddressHasInvalidGeometryMethodException => new TicketError(
                    ValidationErrors.Common.PositionGeometryMethod.Invalid.Message,
                    ValidationErrors.Common.PositionGeometryMethod.Invalid.Code),
                AddressHasInvalidGeometrySpecificationException => new TicketError(
                    ValidationErrors.Common.PositionSpecification.Invalid.Message,
                    ValidationErrors.Common.PositionSpecification.Invalid.Code),
                BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException =>
                    ValidationErrors.Propose.BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCode.ToTicketError(),
                MergedAddressPersistentLocalIdIsInvalidException =>
                    new TicketError("MergedAddressPersistentLocalIdIsInvalid", "MergedAddressPersistentLocalIdIsInvalid"),
                _ => null
            };
        }
    }
}
