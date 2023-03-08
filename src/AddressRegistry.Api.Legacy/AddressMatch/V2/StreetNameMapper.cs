namespace AddressRegistry.Api.Legacy.AddressMatch.V2
{
    using AddressRegistry.Api.Legacy.AddressMatch.Responses;
    using Address;
    using AddressRegistry.Api.Legacy.AddressMatch;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using System.Linq;
    using Consumer.Read.StreetName.Projections;
    using Matching;

    internal class StreetNameMapper : IMapper<StreetNameLatestItem, AddressMatchScoreableItemV2>
    {
        private readonly ResponseOptions _responseOptions;
        private readonly ILatestQueries _latestQueries;

        public StreetNameMapper(ResponseOptions responseOptions, ILatestQueries latestQueries)
        {
            _responseOptions = responseOptions;
            _latestQueries = latestQueries;
        }

        public AddressMatchScoreableItemV2 Map(StreetNameLatestItem source)
        {
            var municipality = _latestQueries.GetAllLatestMunicipalities().Single(x => x.NisCode == source.NisCode);
            var name = Address.AddressMapper.GetDefaultStreetNameName(source, municipality.PrimaryLanguage);
            var homonym = Address.AddressMapper.GetDefaultHomonymAddition(source, municipality.PrimaryLanguage);

            return new AddressMatchScoreableItemV2
            {
                Gemeente = new AdresMatchItemGemeente
                {
                    ObjectId = source.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, source.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipality.DefaultName.Value, municipality.DefaultName.Key))
                },
                Straatnaam = new AdresMatchItemStraatnaam
                {
                    ObjectId = source.PersistentLocalId.ToString(),
                    Detail = string.Format(_responseOptions.StraatnaamDetailUrl, source.PersistentLocalId),
                    Straatnaam = new Straatnaam(new GeografischeNaam(name.Value, name.Key)),
                },
                HomoniemToevoeging = homonym == null ? null : new HomoniemToevoeging(new GeografischeNaam(homonym.Value.Value, homonym.Value.Key))
            };
        }
    }
}
