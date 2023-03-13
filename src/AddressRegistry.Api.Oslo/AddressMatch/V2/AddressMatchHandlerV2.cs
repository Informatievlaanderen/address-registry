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

    public sealed class AddressMatchHandlerV2 : IRequestHandler<AddressMatchRequest, AddressMatchOsloCollection>
    {
        private readonly ILatestQueries _latestQueries;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressMatchHandlerV2(
            ILatestQueries latestQueries,
            IOptions<ResponseOptions> responseOptions)
        {
            _latestQueries = latestQueries;
            _responseOptions = responseOptions;
        }

        public Task<AddressMatchOsloCollection> Handle(AddressMatchRequest request, CancellationToken cancellationToken)
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
                .Select(AdresMatchOsloItem.Create)
                .ToList();

            return Task.FromResult(new AddressMatchOsloCollection
            {
                Context = _responseOptions.Value.ContextUrlAddressMatch,
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
