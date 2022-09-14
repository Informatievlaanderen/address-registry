namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Consumer.Read.Municipality;
    using Microsoft.EntityFrameworkCore;
    using Projections.Syndication;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public class SqsAddressCorrectPostalCodeHandler : SqsLambdaHandler<SqsAddressCorrectPostalCodeRequest>
    {
        private readonly IStreetNames _streetNames;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;

        public SqsAddressCorrectPostalCodeHandler(
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ICommandHandlerResolver bus,
            IStreetNames streetNames,
            IdempotencyContext idempotencyContext,
            SyndicationContext syndicationContext,
            MunicipalityConsumerContext municipalityConsumerContext)
            : base(ticketing, ticketingUrl, bus)
        {
            _streetNames = streetNames;
            _idempotencyContext = idempotencyContext;
            _syndicationContext = syndicationContext;
            _municipalityConsumerContext = municipalityConsumerContext;
        }

        protected override async Task<string> Handle2(SqsAddressCorrectPostalCodeRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(int.Parse(request.MessageGroupId!));
            var addressPersistentLocalId = new AddressPersistentLocalId(request.PersistentLocalId);

            var postInfoIdentifier = request.PostInfoId
                .AsIdentifier()
                .Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            var postalMunicipality = await _syndicationContext.PostalInfoLatestItems.FindAsync(new object[] { postalCode.ToString() }, cancellationToken);
            if (postalMunicipality is null)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }

            var municipality = await _municipalityConsumerContext
                .MunicipalityLatestItems
                .SingleAsync(x => x.NisCode == postalMunicipality.NisCode, cancellationToken);

            var cmd = request.ToCommand(
                streetNamePersistentLocalId,
                postalCode,
                new MunicipalityId(municipality.MunicipalityId),
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            var etag = await GetHash(
                _streetNames,
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                cancellationToken);

            return etag;
        }
    }
}
