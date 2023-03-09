namespace AddressRegistry.Api.Oslo.AddressMatch.V2
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure.Options;
    using Matching;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Requests;
    using Responses;

    public sealed class AddressMatchHandlerV2 : IRequestHandler<AddressMatchRequest, AddressMatchCollection>
    {
        private readonly ILatestQueries _latestQueries;
        private readonly AddressMatchContextV2 _addressMatchContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressMatchHandlerV2(
            ILatestQueries latestQueries,
            AddressMatchContextV2 addressMatchContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _latestQueries = latestQueries;
            _addressMatchContext = addressMatchContext;
            _responseOptions = responseOptions;
        }

        public Task<AddressMatchCollection> Handle(AddressMatchRequest request, CancellationToken cancellationToken)
        {
            const int maxNumberOfResults = 10;

            var warningLogger = new ValidationMessageWarningLogger();
            var addressMatch = new AddressMatchMatchingAlgorithm<AddressMatchScoreableItemV2>(
                new ManualAddressMatchConfig(_responseOptions.Value.SimilarityThreshold, _responseOptions.Value.MaxStreetNamesThreshold),
                _latestQueries,
                new MunicipalityMapper(_responseOptions.Value),
                new StreetNameMapper(_responseOptions.Value, _latestQueries),
                new AddressMapper(_responseOptions.Value, _latestQueries),
                maxNumberOfResults,
                warningLogger);

            var result = addressMatch
                .Process(new AddressMatchBuilder(Map(request)))
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.ScoreableProperty)
                .Take(maxNumberOfResults)
                .Select(x => AdresMatchItem.Create(x, _addressMatchContext, _responseOptions.Value))
                .ToList();

            return Task.FromResult(new AddressMatchCollection
            {
                AdresMatches = result,
                Warnings = warningLogger.Warnings
            });
        }

        private static AddressMatchQueryComponents Map(AddressMatchRequest request) =>
            new AddressMatchQueryComponents
            {
                MunicipalityName = request.Gemeentenaam,
                HouseNumber = request.Huisnummer,
                BoxNumber = request.Busnummer,
                NisCode = request.Niscode,
                PostalCode = request.Postcode,
                StreetName = request.Straatnaam
            };
    }
}
