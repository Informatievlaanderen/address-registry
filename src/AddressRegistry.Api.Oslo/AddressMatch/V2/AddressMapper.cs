namespace AddressRegistry.Api.Oslo.AddressMatch.V2
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Convertors;
    using Infrastructure.Options;
    using Matching;
    using Projections.AddressMatch.AddressDetailV2WithParent;
    using Responses;

    internal class AddressMapper : IMapper<AddressDetailItemV2WithParent, AddressMatchScoreableItemV2>
    {
        private readonly ResponseOptions _responseOptions;
        private readonly ILatestQueries _latestQueries;

        public AddressMapper(ResponseOptions responseOptions, ILatestQueries latestQueries)
        {
            _responseOptions = responseOptions;
            _latestQueries = latestQueries;
        }

        public AddressMatchScoreableItemV2 Map(AddressDetailItemV2WithParent source)
        {
            var streetName = _latestQueries.GetAllLatestStreetNamesByPersistentLocalId()[source.StreetNamePersistentLocalId];
            var municipality = _latestQueries.GetAllLatestMunicipalities()[streetName.NisCode];
            var defaultStreetName = Address.AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);
            var homonym = Address.AddressMapper.GetDefaultHomonymAddition(streetName, municipality.PrimaryLanguage);

            return new AddressMatchScoreableItemV2
            {
                AddressPersistentLocalId = source.AddressPersistentLocalId,
                Identificator = new AdresIdentificator(_responseOptions.Naamruimte, source.AddressPersistentLocalId.ToString(), source.VersionTimestamp.ToBelgianDateTimeOffset()),
                Detail = string.Format(_responseOptions.DetailUrl, source.AddressPersistentLocalId.ToString()),
                Gemeente = new AdresMatchOsloItemGemeente
                {
                    ObjectId = municipality.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, municipality.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipality.DefaultName.Value, municipality.DefaultName.Key))
                },
                Straatnaam = new AdresMatchOsloItemStraatnaam
                {
                    ObjectId = streetName.PersistentLocalId.ToString(),
                    Detail = string.Format(_responseOptions.StraatnaamDetailUrl, streetName.PersistentLocalId),
                    Straatnaam = new Straatnaam(new GeografischeNaam(defaultStreetName.Value, defaultStreetName.Key)),
                },
                HomoniemToevoeging = homonym == null ? null : new HomoniemToevoeging(new GeografischeNaam(homonym.Value.Value, homonym.Value.Key)),
                Postinfo = string.IsNullOrWhiteSpace(source.PostalCode) ? null : new AdresMatchOsloItemPostinfo
                {
                    ObjectId = source.PostalCode,
                    Detail = string.Format(_responseOptions.PostInfoDetailUrl, source.PostalCode),
                },
                Huisnummer = source.HouseNumber,
                Busnummer = source.BoxNumber,
                VolledigAdres = Address.AddressMapper.GetVolledigAdres(source.HouseNumber, source.BoxNumber, source.PostalCode, streetName, municipality),
                AdresPositie = Address.AddressMapper.GetAddressPoint(source.Position,  source.PositionMethod, source.PositionSpecification),
                AdresStatus = source.Status.ConvertFromAddressStatus(),
                OfficieelToegekend = source.OfficiallyAssigned,
            };
        }
    }
}
