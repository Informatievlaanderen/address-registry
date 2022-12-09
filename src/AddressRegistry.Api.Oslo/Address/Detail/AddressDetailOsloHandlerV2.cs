namespace AddressRegistry.Api.Oslo.Address.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public sealed class AddressDetailOsloHandlerV2 : IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;
        private readonly StreetNameConsumerContext _streetNameConsumerContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressDetailOsloHandlerV2(
            LegacyContext legacyContext,
            MunicipalityConsumerContext municipalityConsumerContext,
            StreetNameConsumerContext streetNameConsumerContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _municipalityConsumerContext = municipalityConsumerContext;
            _streetNameConsumerContext = streetNameConsumerContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressDetailOsloResponse> Handle(AddressDetailOsloRequest request, CancellationToken cancellationToken)
        {
            var addressV2 = await _legacyContext
                .AddressDetailV2
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

            var streetNameV2 =
                await _streetNameConsumerContext.StreetNameLatestItems.SingleAsync(
                    x => x.PersistentLocalId == addressV2.StreetNamePersistentLocalId, cancellationToken);

            var municipalityV2 = await _municipalityConsumerContext
                .MunicipalityLatestItems.FirstAsync(m => m.NisCode == streetNameV2.NisCode, cancellationToken);
            var defaultMunicipalityNameV2 = AddressMapper.GetDefaultMunicipalityName(municipalityV2);
            var defaultStreetNameV2 = AddressMapper.GetDefaultStreetNameName(streetNameV2, municipalityV2.PrimaryLanguage);
            var defaultHomonymAdditionV2 = AddressMapper.GetDefaultHomonymAddition(streetNameV2, municipalityV2.PrimaryLanguage);

            var gemeenteV2 = new AdresDetailGemeente(
                municipalityV2.NisCode,
                string.Format(_responseOptions.Value.GemeenteDetailUrl, municipalityV2.NisCode),
                new GeografischeNaam(defaultMunicipalityNameV2.Value, defaultMunicipalityNameV2.Key));

            var straatV2 = new AdresDetailStraatnaam(
                streetNameV2.PersistentLocalId.ToString(),
                string.Format(_responseOptions.Value.StraatnaamDetailUrl, streetNameV2.PersistentLocalId),
                new GeografischeNaam(defaultStreetNameV2.Value, defaultStreetNameV2.Key));

            var postInfoV2 = string.IsNullOrEmpty(addressV2.PostalCode)
                ? null
                : new AdresDetailPostinfo(
                    addressV2.PostalCode,
                    string.Format(_responseOptions.Value.PostInfoDetailUrl, addressV2.PostalCode));

            var homoniemToevoegingV2 = defaultHomonymAdditionV2 == null
                ? null
                : new HomoniemToevoeging(new GeografischeNaam(defaultHomonymAdditionV2.Value.Value, defaultHomonymAdditionV2.Value.Key));

            return new AddressDetailOsloResponse(
                _responseOptions.Value.Naamruimte,
                _responseOptions.Value.ContextUrlDetail,
                addressV2.AddressPersistentLocalId.ToString(),
                addressV2.HouseNumber,
                addressV2.BoxNumber,
                gemeenteV2,
                straatV2,
                homoniemToevoegingV2,
                postInfoV2,
                AddressMapper.GetAddressPoint(
                    addressV2.Position,
                    addressV2.PositionMethod,
                    addressV2.PositionSpecification),
                AddressMapper.ConvertFromAddressStatus(addressV2.Status),
                defaultStreetNameV2.Key,
                addressV2.OfficiallyAssigned,
                addressV2.VersionTimestamp.ToBelgianDateTimeOffset(),
                addressV2.LastEventHash);
        }
    }
}
