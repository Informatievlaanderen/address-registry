namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Projections.Legacy;

    public sealed record CrabHouseNumberCountRequest : IRequest<TotaalAantalResponse>;

    public sealed class CrabHouseNumberCountHandler : IRequestHandler<CrabHouseNumberCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;

        public CrabHouseNumberCountHandler(LegacyContext legacyContext)
        {
            _legacyContext = legacyContext;
        }

        public Task<TotaalAantalResponse> Handle(CrabHouseNumberCountRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TotaalAantalResponse
            {
                Aantal = new CrabHouseNumberQuery(_legacyContext).Count()
            });
        }
    }
}
