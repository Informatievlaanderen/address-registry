namespace AddressRegistry.Api.Legacy.AddressMatch.V1
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.Legacy.AddressMatch.Requests;
    using AddressRegistry.Api.Legacy.AddressMatch.Responses;
    using AddressRegistry.Api.Legacy.Infrastructure.Options;
    using Matching;
    using MediatR;
    using Microsoft.Extensions.Options;

    public sealed class AddressMatchHandler : IRequestHandler<AddressMatchRequest, AddressMatchCollection>
    {
        private readonly IKadRrService _kadRrService;
        private readonly ILatestQueries _latestQueries;
        private readonly AddressMatchContext _addressMatchContext;
        private readonly BuildingContext _buildingContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressMatchHandler(
            IKadRrService kadRrService,
            ILatestQueries latestQueries,
            AddressMatchContext addressMatchContext,
            BuildingContext buildingContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _kadRrService = kadRrService;
            _latestQueries = latestQueries;
            _addressMatchContext = addressMatchContext;
            _buildingContext = buildingContext;
            _responseOptions = responseOptions;
        }

        public Task<AddressMatchCollection> Handle(AddressMatchRequest request, CancellationToken cancellationToken)
        {
            const int maxNumberOfResults = 10;

            var warningLogger = new ValidationMessageWarningLogger();
            var addressMatch = new AddressMatchMatchingAlgorithm<AdresMatchScorableItem>(
                _kadRrService,
                new ManualAddressMatchConfig(_responseOptions.Value.SimilarityThreshold, _responseOptions.Value.MaxStreetNamesThreshold),
                _latestQueries,
                new GemeenteMapper(_responseOptions.Value),
                new StreetNameMapper(_responseOptions.Value, _latestQueries),
                new AdresMapper(_responseOptions.Value, _latestQueries),
                maxNumberOfResults,
                warningLogger);

            var result = addressMatch
                .Process(new AddressMatchBuilder(Map(request)))
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.ScoreableProperty)
                .Take(maxNumberOfResults)
                .Select(x => AdresMatchItem.Create(x, _buildingContext, _addressMatchContext, _responseOptions.Value))
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
                Index = request.Index,
                KadStreetNameCode = request.KadStraatcode,
                NisCode = request.Niscode,
                PostalCode = request.Postcode,
                RrStreetCode = request.RrStraatcode,
                StreetName = request.Straatnaam
            };
    }
}
