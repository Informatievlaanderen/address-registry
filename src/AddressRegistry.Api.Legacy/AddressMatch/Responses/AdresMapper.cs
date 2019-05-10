namespace AddressRegistry.Api.Legacy.AddressMatch.Responses
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

    internal class AdresMapper : IMapper<AddressDetailItem, AdresMatchItem>
    {
        private readonly ResponseOptions _responseOptions;
        private readonly ILatestQueries _latestQueries;

        public AdresMapper(ResponseOptions responseOptions, ILatestQueries latestQueries)
        {
            _responseOptions = responseOptions;
            _latestQueries = latestQueries;
        }

        public AdresMatchItem Map(AddressDetailItem source)
        {
            var streetName = _latestQueries.GetAllLatestStreetNames().Single(x => x.StreetNameId == source.StreetNameId);
            var municipality = _latestQueries.GetAllLatestMunicipalities().Single(x => x.NisCode == streetName.NisCode);
            var defaultStreetName = AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);
            var homonym = AddressMapper.GetDefaultHomonymAddition(streetName, municipality.PrimaryLanguage);

            return new AdresMatchItem
            {
                Identificator = new Identificator(_responseOptions.Naamruimte, source.OsloId.ToString(), source.VersionTimestamp.ToBelgianDateTimeOffset()),
                Detail = string.Format(_responseOptions.DetailUrl, source.OsloId.Value.ToString()),
                Gemeente = new AdresMatchItemGemeente
                {
                    ObjectId = municipality.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, municipality.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipality.DefaultName.Value, municipality.DefaultName.Key))
                },
                Straatnaam = new AdresMatchItemStraatnaam
                {
                    ObjectId = streetName.OsloId,
                    Detail = string.Format(_responseOptions.StraatnaamDetailUrl, source.OsloId),
                    Straatnaam = new Straatnaam(new GeografischeNaam(defaultStreetName.Value, defaultStreetName.Key)),
                },
                HomoniemToevoeging = homonym == null ? null : new HomoniemToevoeging(new GeografischeNaam(homonym.Value.Value, homonym.Value.Key)),
                Postinfo = new AdresMatchItemPostinfo
                {
                    ObjectId = source.PostalCode,
                    Detail = string.Format(_responseOptions.PostInfoDetailUrl, source.PostalCode),
                },
                Huisnummer = source.HouseNumber,
                Busnummer = source.BoxNumber,
                VolledigAdres = AddressMapper.GetVolledigAdres(source.HouseNumber, source.BoxNumber, source.PostalCode, streetName, municipality),
                AdresPositie = AddressMapper.GetAdresPoint(source),
                PositieSpecificatie = AddressMapper.ConvertFromGeometrySpecification(source.PositionSpecification),
                PositieGeometrieMethode = AddressMapper.ConvertFromGeometryMethod(source.PositionMethod),
                AdresStatus = AddressMapper.ConvertFromAddressStatus(source.Status),
                OfficieelToegekend = source.OfficiallyAssigned,
                //AdresseerbareObjecten = adresDetail.GebouweenheidObjectIds.Select(gObjectID => new AdresseerbaarObject
                //{
                //    ObjectId = gObjectID.ToString(),
                //    ObjectType = ObjectType.Gebouweenheid,
                //    Detail = _responseOptions.Link(GebouweenheidController.GebouweenheidGetLatestVersionRouteName, new { ObjectId = gObjectID.ToString() })
                //}).Concat(adresDetail.PerceelCapaKeys.Select(pCapaKey => new AdresseerbaarObject
                //{
                //    ObjectId = pCapaKey,
                //    ObjectType = ObjectType.Perceel,
                //    Detail = _responseOptions.Link(PerceelController.PerceelGetLatestVersionRouteName, new { ObjectId = pCapaKey })
                //})).ToList()
            };

        }
    }
}
