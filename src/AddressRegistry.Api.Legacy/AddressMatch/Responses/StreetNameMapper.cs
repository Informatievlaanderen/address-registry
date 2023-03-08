namespace AddressRegistry.Api.Legacy.AddressMatch.Responses
{
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Projections.Syndication.StreetName;
    using System.Linq;
    using V1.Matching;

    internal class StreetNameMapper : IMapper<StreetNameLatestItem, AdresMatchScorableItem>
    {
        private readonly ResponseOptions _responseOptions;
        private readonly ILatestQueries _latestQueries;

        public StreetNameMapper(ResponseOptions responseOptions, ILatestQueries latestQueries)
        {
            _responseOptions = responseOptions;
            _latestQueries = latestQueries;
        }

        public AdresMatchScorableItem Map(StreetNameLatestItem source)
        {
            var municipality = _latestQueries.GetAllLatestMunicipalities().Single(x => x.NisCode == source.NisCode);
            var name = AddressMapper.GetDefaultStreetNameName(source, municipality.PrimaryLanguage);
            var homonym = AddressMapper.GetDefaultHomonymAddition(source, municipality.PrimaryLanguage);

            return new AdresMatchScorableItem
            {
                Gemeente = new AdresMatchItemGemeente
                {
                    ObjectId = source.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, source.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipality.DefaultName.Value, municipality.DefaultName.Key))
                },
                Straatnaam = new AdresMatchItemStraatnaam
                {
                    ObjectId = source.PersistentLocalId,
                    Detail = string.Format(_responseOptions.StraatnaamDetailUrl, source.PersistentLocalId),
                    Straatnaam = new Straatnaam(new GeografischeNaam(name.Value, name.Key)),
                },
                HomoniemToevoeging = homonym == null ? null : new HomoniemToevoeging(new GeografischeNaam(homonym.Value.Value, homonym.Value.Key))
            };
        }
    }
}
