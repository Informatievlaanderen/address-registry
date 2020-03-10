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
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.AddressDetail;

    internal class AdresMapper : IMapper<AddressDetailItem, AdresMatchItem>
    {
        private readonly ResponseOptions _responseOptions;
        private readonly ILatestQueries _latestQueries;
        private readonly AddressMatchContext _context;
        private readonly BuildingContext _buildingContext;

        public AdresMapper(ResponseOptions responseOptions, ILatestQueries latestQueries, AddressMatchContext context, BuildingContext buildingContext)
        {
            _responseOptions = responseOptions;
            _latestQueries = latestQueries;
            _context = context;
            _buildingContext = buildingContext;
        }

        public AdresMatchItem Map(AddressDetailItem source)
        {
            var streetName = _latestQueries.GetAllLatestStreetNames().Single(x => x.StreetNameId == source.StreetNameId);
            var municipality = _latestQueries.GetAllLatestMunicipalities().Single(x => x.NisCode == streetName.NisCode);
            var defaultStreetName = AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);
            var homonym = AddressMapper.GetDefaultHomonymAddition(streetName, municipality.PrimaryLanguage);

            return new AdresMatchItem
            {
                Identificator = new AdresIdentificator(_responseOptions.Naamruimte, source.PersistentLocalId.ToString(), source.VersionTimestamp.ToBelgianDateTimeOffset()),
                Detail = string.Format(_responseOptions.DetailUrl, source.PersistentLocalId.Value.ToString()),
                Gemeente = new AdresMatchItemGemeente
                {
                    ObjectId = municipality.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, municipality.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipality.DefaultName.Value, municipality.DefaultName.Key))
                },
                Straatnaam = new AdresMatchItemStraatnaam
                {
                    ObjectId = streetName.PersistentLocalId,
                    Detail = string.Format(_responseOptions.StraatnaamDetailUrl, streetName.PersistentLocalId),
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
                AdresPositie = AddressMapper.GetAddressPoint(source.Position),
                PositieSpecificatie = AddressMapper.ConvertFromGeometrySpecification(source.PositionSpecification),
                PositieGeometrieMethode = AddressMapper.ConvertFromGeometryMethod(source.PositionMethod),
                AdresStatus = AddressMapper.ConvertFromAddressStatus(source.Status),
                OfficieelToegekend = source.OfficiallyAssigned,
                AdresseerbareObjecten =
                    _buildingContext.BuildingUnits
                        .Include(x => x.Addresses)
                        .Where(x => x.Addresses.Any(y => y.AddressId == source.AddressId) && !x.IsRemoved && x.IsBuildingComplete && x.IsComplete && x.PersistentLocalId.HasValue)
                        .Select(x => new { x.PersistentLocalId })
                        .ToList()
                        .Select(matchLatestItem => new AdresseerbaarObject
                        {
                            ObjectId = matchLatestItem.PersistentLocalId.ToString(),
                            ObjectType = ObjectType.Gebouweenheid,
                            Detail = string.Format(_responseOptions.GebouweenheidDetailUrl, matchLatestItem.PersistentLocalId.ToString()),
                        })
                        .ToList()
                        .Concat(
                            _context.ParcelAddressMatchLatestItems
                            .Where(x => x.AddressId == source.AddressId && !x.IsRemoved)
                            .ToList()
                            .Select(matchLatestItem => new AdresseerbaarObject
                            {
                                ObjectId = matchLatestItem.ParcelPersistentLocalId,
                                ObjectType = ObjectType.Perceel,
                                Detail = string.Format(_responseOptions.PerceelDetailUrl, matchLatestItem.ParcelPersistentLocalId),
                            })
                            .ToList())
                        .ToList()
            };
        }
    }
}
