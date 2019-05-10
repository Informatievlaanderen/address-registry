namespace AddressRegistry.Api.Legacy.AddressMatch.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Infrastructure.Options;
    using Matching;
    using Projections.Syndication.Municipality;

    internal class GemeenteMapper : IMapper<MunicipalityLatestItem, AdresMatchItem>
    {
        private readonly ResponseOptions _responseOptions;

        public GemeenteMapper(ResponseOptions responseOptions)
        {
            _responseOptions = responseOptions;
        }

        public AdresMatchItem Map(MunicipalityLatestItem source)
        {
            return new AdresMatchItem
            {
                Gemeente = new AdresMatchItemGemeente
                {
                    ObjectId = source.NisCode,
                    Detail = string.Format(_responseOptions.DetailUrl, source.NisCode),
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam(source.DefaultName.Value, source.DefaultName.Key))
                },
            };
        }
    }
}
