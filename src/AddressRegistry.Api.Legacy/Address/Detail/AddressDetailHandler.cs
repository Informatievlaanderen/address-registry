namespace AddressRegistry.Api.Legacy.Address.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public sealed record AddressDetailRequest(int PersistentLocalId) : IRequest<AddressDetailResponse>;

    public sealed class AddressDetailHandler : IRequestHandler<AddressDetailRequest, AddressDetailResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressDetailHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressDetailResponse> Handle(AddressDetailRequest request, CancellationToken cancellationToken)
        {
            var address = await _legacyContext
                .AddressDetail
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (address is not null && address.Removed)
            {
                throw new ApiException("Adres werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (address is null || !address.Complete)
            {
                throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);
            }

            var streetName = await _syndicationContext.StreetNameLatestItems.FindAsync(new object[] { address.StreetNameId }, cancellationToken);
            var municipality = await _syndicationContext.MunicipalityLatestItems.FirstAsync(m => m.NisCode == streetName.NisCode, cancellationToken);
            var defaultMunicipalityName = AddressMapper.GetDefaultMunicipalityName(municipality);
            var defaultStreetName = AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);
            var defaultHomonymAddition = AddressMapper.GetDefaultHomonymAddition(streetName, municipality.PrimaryLanguage);

            var gemeente = new AdresDetailGemeente(
                municipality.NisCode,
                string.Format(_responseOptions.Value.GemeenteDetailUrl, municipality.NisCode),
                new GeografischeNaam(defaultMunicipalityName.Value, defaultMunicipalityName.Key));

            var straat = new AdresDetailStraatnaam(
                streetName.PersistentLocalId,
                string.Format(_responseOptions.Value.StraatnaamDetailUrl, streetName.PersistentLocalId),
                new GeografischeNaam(defaultStreetName.Value, defaultStreetName.Key));

            var postInfo = string.IsNullOrEmpty(address.PostalCode)
                ? null
                : new AdresDetailPostinfo(
                    address.PostalCode,
                    string.Format(_responseOptions.Value.PostInfoDetailUrl, address.PostalCode));

            var homoniemToevoeging = defaultHomonymAddition == null
                ? null
                : new HomoniemToevoeging(new GeografischeNaam(defaultHomonymAddition.Value.Value, defaultHomonymAddition.Value.Key));

            return new AddressDetailResponse(
                        _responseOptions.Value.Naamruimte,
                        address.PersistentLocalId.ToString(),
                        address.HouseNumber,
                        address.BoxNumber,
                        gemeente,
                        straat,
                        homoniemToevoeging,
                        postInfo,
                        AddressMapper.GetAddressPoint(address.Position),
                        AddressMapper.ConvertFromGeometryMethod(address.PositionMethod),
                        AddressMapper.ConvertFromGeometrySpecification(address.PositionSpecification),
                        AddressMapper.ConvertFromAddressStatus(address.Status),
                        defaultStreetName.Key,
                        address.OfficiallyAssigned,
                        address.VersionTimestamp.ToBelgianDateTimeOffset());
        }
    }
}
