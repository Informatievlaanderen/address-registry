namespace AddressRegistry.Api.Oslo.Address.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;

    public sealed class AddressCountOsloHandlerV2 : IRequestHandler<AddressCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly AddressQueryContext _addressQueryContext;

        public AddressCountOsloHandlerV2(
            LegacyContext legacyContext,
            AddressQueryContext addressQueryContext)
        {
            _legacyContext = legacyContext;
            _addressQueryContext = addressQueryContext;
        }

        public async Task<TotaalAantalResponse> Handle(AddressCountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.Filtering.ShouldFilter
                    ? await new AddressListOsloQueryV2(_addressQueryContext)
                        .Fetch(request.Filtering, request.Sorting, request.Pagination)
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_legacyContext
                        .AddressListViewCountV2
                        .First()
                        .Count)
            };
        }
    }
}
