namespace AddressRegistry.Api.Legacy.CrabSubaddress
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Projections.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;

    public sealed record CrabSubaddressCountRequest : IRequest<TotaalAantalResponse>;

    public sealed class CrabSubaddressCountHandler : IRequestHandler<CrabSubaddressCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;

        public CrabSubaddressCountHandler(LegacyContext legacyContext)
        {
            _legacyContext = legacyContext;
        }

        public Task<TotaalAantalResponse> Handle(CrabSubaddressCountRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TotaalAantalResponse
            {
                Aantal = new CrabSubaddressQuery(_legacyContext).Count()
            });
        }
    }
}
