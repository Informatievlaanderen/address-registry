namespace AddressRegistry.Api.Oslo.AddressMatch.V1
{
    using AddressRegistry.Projections.Syndication.Municipality;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Infrastructure.Options;
    using Matching;
    using Responses;

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
                Gemeente = new AdresMatchOsloItemGemeente
                {
                    ObjectId = source.NisCode,
                    Detail = string.Format(_responseOptions.GemeenteDetailUrl, source.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(source.DefaultName.Value, source.DefaultName.Key))
                },
            };
    }
}
