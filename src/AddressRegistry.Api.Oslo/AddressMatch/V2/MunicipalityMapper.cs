namespace AddressRegistry.Api.Oslo.AddressMatch.V2
{
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Infrastructure.Options;
    using Matching;
    using Responses;

    internal sealed class MunicipalityMapper : IMapper<MunicipalityLatestItem, AddressMatchScoreableItemV2>
    {
        private readonly ResponseOptionsV2 _responseOptions;

        public MunicipalityMapper(ResponseOptionsV2 responseOptions)
        {
            _responseOptions = responseOptions;
        }

        public AddressMatchScoreableItemV2 Map(MunicipalityLatestItem source) =>
            new AddressMatchScoreableItemV2
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
