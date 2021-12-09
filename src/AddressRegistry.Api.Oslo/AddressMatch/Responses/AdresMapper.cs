namespace AddressRegistry.Api.Oslo.AddressMatch.Responses
{
    using System.Linq;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Matching;
    using Projections.Legacy.AddressDetail;

    internal class AdresMapper : IMapper<AddressDetailItem, AdresMatchScorableItem>
    {
        private readonly ResponseOptions _responseOptions;
        private readonly ILatestQueries _latestQueries;

        public AdresMapper(ResponseOptions responseOptions, ILatestQueries latestQueries)
        {
            _responseOptions = responseOptions;
            _latestQueries = latestQueries;
        }

        public AdresMatchScorableItem Map(AddressDetailItem source)
        {
            var streetName = _latestQueries.GetAllLatestStreetNames().Single(x => x.StreetNameId == source.StreetNameId);
            var municipality = _latestQueries.GetAllLatestMunicipalities().Single(x => x.NisCode == streetName.NisCode);
            var defaultStreetName = AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);
            var homonym = AddressMapper.GetDefaultHomonymAddition(streetName, municipality.PrimaryLanguage);

            return new AdresMatchScorableItem
            {
                AddressId = source.AddressId,
                Identificator = new AdresIdentificator(_responseOptions.Naamruimte, source.PersistentLocalId.ToString(), source.VersionTimestamp.ToBelgianDateTimeOffset()),
                Detail = string.Format(_responseOptions.DetailUrl, source.PersistentLocalId.Value.ToString()),
                Gemeente = new AdresMatchOsloItemGemeente
                {
                    ObjectId = municipality.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, municipality.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipality.DefaultName.Value, municipality.DefaultName.Key))
                },
                Straatnaam = new AdresMatchOsloItemStraatnaam
                {
                    ObjectId = streetName.PersistentLocalId,
                    Detail = string.Format(_responseOptions.StraatnaamDetailUrl, streetName.PersistentLocalId),
                    Straatnaam = new Straatnaam(new GeografischeNaam(defaultStreetName.Value, defaultStreetName.Key)),
                },
                HomoniemToevoeging = homonym == null ? null : new HomoniemToevoeging(new GeografischeNaam(homonym.Value.Value, homonym.Value.Key)),
                Postinfo = new AdresMatchOsloItemPostinfo
                {
                    ObjectId = source.PostalCode,
                    Detail = string.Format(_responseOptions.PostInfoDetailUrl, source.PostalCode),
                },
                Huisnummer = source.HouseNumber,
                Busnummer = source.BoxNumber,
                VolledigAdres = AddressMapper.GetVolledigAdres(source.HouseNumber, source.BoxNumber, source.PostalCode, streetName, municipality),
                AdresPositie = AddressMapper.GetAddressPoint(source.Position, source.PositionMethod, source.PositionSpecification),
                PositieSpecificatie = AddressMapper.ConvertFromGeometrySpecification(source.PositionSpecification),
                PositieGeometrieMethode = AddressMapper.ConvertFromGeometryMethod(source.PositionMethod),
                AdresStatus = AddressMapper.ConvertFromAddressStatus(source.Status),
                OfficieelToegekend = source.OfficiallyAssigned,
            };
        }
    }
}
