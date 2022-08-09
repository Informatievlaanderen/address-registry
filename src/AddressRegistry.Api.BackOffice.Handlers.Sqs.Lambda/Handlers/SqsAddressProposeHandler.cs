namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Requests;
    using Address;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Projections.Syndication;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;
    using PostalCode = StreetName.PostalCode;

    public class SqsAddressProposeHandler : SqsLambdaHandler<SqsAddressProposeRequest>
    {
        private readonly IStreetNames _streetNames;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public SqsAddressProposeHandler(
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ICommandHandlerResolver bus,
            IStreetNames streetNames,
            BackOfficeContext backOfficeContext,
            IdempotencyContext idempotencyContext,
            SyndicationContext syndicationContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator
            )
            : base(ticketing, ticketingUrl, bus)
        {
            _streetNames = streetNames;
            _backOfficeContext = backOfficeContext;
            _idempotencyContext = idempotencyContext;
            _syndicationContext = syndicationContext;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        protected override async Task<string> Handle2(SqsAddressProposeRequest request, CancellationToken cancellationToken)
        {
            var identifier = request.StraatNaamId
            .AsIdentifier()
            .Map(x => x);

            var postInfoIdentifier = request.PostInfoId
                .AsIdentifier()
                .Map(x => x);

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(int.Parse(identifier.Value));
            var postalCodeId = new PostalCode(postInfoIdentifier.Value);
            var addressPersistentLocalId =
                new AddressPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

            var postalMunicipality = await _syndicationContext.PostalInfoLatestItems.FindAsync(new object[] { postalCodeId.ToString() }, cancellationToken);
            if (postalMunicipality is null)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }

            var municipality = await _syndicationContext
                .MunicipalityLatestItems
                .SingleAsync(x => x.NisCode == postalMunicipality.NisCode, cancellationToken);

            var cmd = request.ToCommand(
                streetNamePersistentLocalId,
                postalCodeId,
                new MunicipalityId(municipality.MunicipalityId),
                addressPersistentLocalId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(_idempotencyContext, cmd.CreateCommandId(), cmd, request.Metadata, cancellationToken);

            // Insert PersistentLocalId with MunicipalityId
            await _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .AddAsync(
                    new AddressPersistentIdStreetNamePersistentId(
                        addressPersistentLocalId,
                        streetNamePersistentLocalId),
                    cancellationToken);
            await _backOfficeContext.SaveChangesAsync(cancellationToken);

            var addressHash = await GetHash(
                _streetNames,
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                cancellationToken);

            return addressHash;
        }
    }
}
