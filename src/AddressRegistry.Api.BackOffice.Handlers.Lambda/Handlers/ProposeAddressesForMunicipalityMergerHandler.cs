namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Validation;
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

    public sealed class ProposeAddressesForMunicipalityMergerHandler : SqsLambdaHandler<ProposeAddressesForMunicipalityMergerLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public ProposeAddressesForMunicipalityMergerHandler(
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
            //TODO-rik implement
            /*  // public IEnumerable<ProposeAddressForMunicipalityMerger> ToCommand()
        // {
        //     return _addresses.Select(x =>
        //         new ProposeAddressForMunicipalityMerger(
        //             this.StreetNamePersistentLocalId(),
        //             new PostalCode(x.PostalCode),
        //             new AddressPersistentLocalId(x.NewAddressPersistentLocalId),
        //             HouseNumber.Create(x.HouseNumber),
        //             x.BoxNumber is not null ? new BoxNumber(x.BoxNumber) : null,
        //             x.GeometryMethod,
        //             x.GeometrySpecification,
        //             new ExtendedWkbGeometry(x.Position),
        //             x.OfficiallyAssigned,
        //             new AddressPersistentLocalId(x.MergedAddressPersistentLocalId),
        //             Provenance));
        // }*/
            var commands = new List<ProposeAddressForMunicipalityMerger>(); //request.ToCommand().ToList();

            try
            {
                foreach (var command in commands)
                {
                    await IdempotentCommandHandler.Dispatch(
                        command.CreateCommandId(),
                        command,
                        request.Metadata,
                        cancellationToken);
                }
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var etagResponses = new List<ETagResponse>();

            foreach (var command in commands)
            {
                await _backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    command.AddressPersistentLocalId, request.StreetNamePersistentLocalId(), cancellationToken);

                var lastHash = await GetHash(request.StreetNamePersistentLocalId(), command.AddressPersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, command.AddressPersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ProposeAddressesForMunicipalityMergerLambdaRequest request)
        {
            return exception switch
            {
                //TODO-rik confirm
                // StreetNameIsRemovedException => new TicketError(
                //     ValidationErrors.Common.StreetNameInvalid.Message(request.Request.StraatNaamId),
                //     ValidationErrors.Common.StreetNameInvalid.Code),
                ParentAddressAlreadyExistsException => new TicketError(
                    ValidationErrors.Common.AddressAlreadyExists.Message,
                    ValidationErrors.Common.AddressAlreadyExists.Code),
                HouseNumberHasInvalidFormatException => new TicketError(
                    ValidationErrors.Common.HouseNumberInvalidFormat.Message,
                    ValidationErrors.Common.HouseNumberInvalidFormat.Code),
                AddressAlreadyExistsException => new TicketError(
                    ValidationErrors.Common.AddressAlreadyExists.Message,
                    ValidationErrors.Common.AddressAlreadyExists.Code),
                //TODO-rik confirm
                // ParentAddressNotFoundException e => new TicketError(
                //     ValidationErrors.Propose.AddressHouseNumberUnknown.Message(request.Request.StraatNaamId, e.HouseNumber),
                //     ValidationErrors.Propose.AddressHouseNumberUnknown.Code),
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
                //TODO-rik handle validation for MergedAddressPersistentLocalId
                // MergedStreetNamePersistentLocalIdsAreMissingException =>
                //     new TicketError("MergedStreetNamePersistentLocalIdsAreMissing", "MergedStreetNamePersistentLocalIdsAreMissing"),
                // MergedStreetNamePersistentLocalIdsAreNotUniqueException =>
                //     new TicketError("MergedStreetNamePersistentLocalIdsAreNotUnique", "MergedStreetNamePersistentLocalIdsAreNotUnique"),
                _ => null
            };
        }
    }
}
