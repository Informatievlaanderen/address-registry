namespace AddressRegistry.Api.Oslo.Address.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Consumer.Read.Municipality.Projections;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public sealed class AddressListOsloHandlerV2 : IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>
    {
        private readonly AddressQueryContext _addressQueryContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressListOsloHandlerV2(
            AddressQueryContext addressQueryContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressQueryContext = addressQueryContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressListOsloResponse> Handle(AddressListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagedAddressesV2 = new AddressListOsloQueryV2(_addressQueryContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var addressesV2 = await pagedAddressesV2.Items
                    .Select(a =>
                    new
                    {
                        a.AddressPersistentLocalId,
                        a.StreetNamePersistentLocalId,
                        a.HouseNumber,
                        a.BoxNumber,
                        a.PostalCode,
                        a.Status,
                        a.VersionTimestamp
                    })
                    .ToListAsync(cancellationToken);

            var streetNameIdsV2 = addressesV2
                .Select(x => x.StreetNamePersistentLocalId)
                .Distinct()
                .ToList();

            var streetNamesV2 = await _addressQueryContext
                .StreetNameConsumerLatestItems
                .Where(x => streetNameIdsV2.Contains(x.PersistentLocalId))
                .ToListAsync(cancellationToken);

            var nisCodesV2 = streetNamesV2
                .Select(x => x.NisCode)
                .Distinct()
                .ToList();

            var municipalitiesV2 = await _addressQueryContext
                .MunicipalityConsumerLatestItems
                .Where(x => nisCodesV2.Contains(x.NisCode))
                .ToListAsync(cancellationToken);

            var addressListItemResponsesV2 = addressesV2
                .Select(a =>
                {
                    var streetName = streetNamesV2.SingleOrDefault(x => x.PersistentLocalId == a.StreetNamePersistentLocalId);
                    MunicipalityLatestItem? municipality = null;
                    if (streetName != null)
                    {
                        municipality = municipalitiesV2.SingleOrDefault(x => x.NisCode == streetName.NisCode);
                    }

                    return new AddressListItemOsloResponse(
                        a.AddressPersistentLocalId,
                        _responseOptions.Value.Naamruimte,
                        _responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        a.BoxNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                        AddressMapper.ConvertFromAddressStatus(a.Status),
                        a.VersionTimestamp.ToBelgianDateTimeOffset());
                })
                .ToList();

            return new AddressListOsloResponse
            {
                Adressen = addressListItemResponsesV2,
                Volgende = pagedAddressesV2.PaginationInfo.BuildNextUri(addressListItemResponsesV2.Count, _responseOptions.Value.VolgendeUrl),
                Context = _responseOptions.Value.ContextUrlList,
                Sorting = pagedAddressesV2.Sorting,
                Pagination = pagedAddressesV2.PaginationInfo
            };
        }
    }
}