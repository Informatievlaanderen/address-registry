namespace AddressRegistry.Api.Oslo.Address.V3.Detail
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public sealed class AddressDetailOsloHandler : IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloV3Response>
    {
        private readonly LegacyContext _legacyContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;
        private readonly StreetNameConsumerContext _streetNameConsumerContext;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public AddressDetailOsloHandler(
            LegacyContext legacyContext,
            MunicipalityConsumerContext municipalityConsumerContext,
            StreetNameConsumerContext streetNameConsumerContext,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _legacyContext = legacyContext;
            _municipalityConsumerContext = municipalityConsumerContext;
            _streetNameConsumerContext = streetNameConsumerContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressDetailOsloV3Response> Handle(AddressDetailOsloRequest request, CancellationToken cancellationToken)
        {
            var addressV2 = await _legacyContext
                .AddressDetailV2WithParent
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.AddressPersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (addressV2 != null && addressV2.Removed)
            {
                throw new ApiException("Adres werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (addressV2 == null)
            {
                throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);
            }

            var adresDetailHuisnummerObject = addressV2.ParentAddressPersistentLocalId.HasValue
                ? new AdresIsDeelVan(
                    addressV2.ParentAddressPersistentLocalId.Value,
                    string.Format(_responseOptions.Value.DetailUrl, addressV2.ParentAddressPersistentLocalId.Value))
                : null;

            var streetNameV2 =
                await _streetNameConsumerContext.StreetNameLatestItems.SingleAsync(
                    x => x.PersistentLocalId == addressV2.StreetNamePersistentLocalId, cancellationToken);

            var municipalityV2 = await _municipalityConsumerContext
                .MunicipalityLatestItems.FirstAsync(m => m.NisCode == streetNameV2.NisCode, cancellationToken);
            var municipalityNamesV2 = AddressMapper.GetMunicipalityNames(municipalityV2);
            var streetNameNamesV2 = AddressMapper.GetStreetNameNames(streetNameV2);
            var homonymAdditionsV2 = AddressMapper.GetHomonymAdditions(streetNameV2);

            var gemeenteV2 = new AdresHeeftGemeentenaam(
                municipalityV2.NisCode,
                string.Format(_responseOptions.Value.GemeenteDetailUrl, municipalityV2.NisCode),
                municipalityNamesV2.ToList());

            var straatV2 = new AdresHeeftStraatnaam(
                streetNameV2.PersistentLocalId.ToString(),
                string.Format(_responseOptions.Value.StraatnaamDetailUrl, streetNameV2.PersistentLocalId),
                streetNameNamesV2.ToList(),
                homonymAdditionsV2?.ToList());

            var postInfoV2 = string.IsNullOrEmpty(addressV2.PostalCode)
                ? null
                : new AdresHeeftPostinfo(
                    addressV2.PostalCode,
                    string.Format(_responseOptions.Value.PostInfoDetailUrl, addressV2.PostalCode));

            return new AddressDetailOsloV3Response(
                _responseOptions.Value.ContextUrlDetail,
                addressV2.AddressPersistentLocalId.ToString(),
                addressV2.HouseNumber,
                adresDetailHuisnummerObject,
                addressV2.BoxNumber,
                gemeenteV2,
                straatV2,
                postInfoV2,
                addressV2.PostalCode,
                AddressMapper.GetAddressPoint(
                    addressV2.Position,
                    addressV2.PositionMethod,
                    addressV2.PositionSpecification),
                AddressMapper.ConvertFromAddressStatus(addressV2.Status),
                addressV2.OfficiallyAssigned,
                addressV2.VersionTimestamp.ToBelgianDateTimeOffset(),
                _responseOptions.Value.DetailUrl,
                _responseOptions.Value.AddressDetailParcelsLink,
                _responseOptions.Value.AddressDetailBuildingUnitsLink,
                addressV2.LastEventHash);
        }
    }
}
