using AddressRegistry.Api.Legacy.AddressMatch.Responses;

namespace AddressRegistry.Api.Legacy.AddressMatch.V1
{
    using AddressRegistry.Api.Legacy.AddressMatch;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Infrastructure.Options;
    using Projections.Syndication.Municipality;
    using V1.Matching;

    internal class GemeenteMapper : IMapper<MunicipalityLatestItem, AdresMatchScorableItem>
    {
        private readonly ResponseOptions _responseOptions;

        public GemeenteMapper(ResponseOptions responseOptions)
        {
            _responseOptions = responseOptions;
        }

        public AdresMatchScorableItem Map(MunicipalityLatestItem source) =>
            new AdresMatchScorableItem
            {
                Gemeente = new AdresMatchItemGemeente
                {
                    ObjectId = source.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, source.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(source.DefaultName.Value, source.DefaultName.Key))
                },
            };
    }
}
