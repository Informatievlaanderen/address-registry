namespace AddressRegistry.Api.Oslo.Address.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;

    public sealed record AddressCountRequest(
        FilteringHeader<AddressFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<TotaalAantalResponse>;

    public sealed class AddressCountOsloHandler : IRequestHandler<AddressCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly AddressQueryContext _addressQueryContext;

        public AddressCountOsloHandler(
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
                    ? await new AddressListOsloQuery(_addressQueryContext)
                        .Fetch(request.Filtering, request.Sorting, request.Pagination)
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_legacyContext
                        .AddressListViewCount
                        .First()
                        .Count)
            };
        }
    }
}
