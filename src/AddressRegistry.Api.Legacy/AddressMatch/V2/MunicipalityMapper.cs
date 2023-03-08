namespace AddressRegistry.Api.Legacy.AddressMatch.V2
{
    using AddressMatch;
    using Responses;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Consumer.Read.Municipality.Projections;
    using Infrastructure.Options;
    using Matching;

    internal class MunicipalityMapper : IMapper<MunicipalityLatestItem, AddressMatchScoreableItemV2>
    {
        private readonly ResponseOptions _responseOptions;

        public MunicipalityMapper(ResponseOptions responseOptions)
        {
            _responseOptions = responseOptions;
        }

        public AddressMatchScoreableItemV2 Map(MunicipalityLatestItem source) =>
            new AddressMatchScoreableItemV2
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
