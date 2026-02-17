namespace AddressRegistry.Api.Oslo.AddressMatch.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Matching;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Requests;
    using Responses;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

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

            var processedResults = addressMatch
                .Process(new AddressMatchBuilder(Map(request)));

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                processedResults = FilterByStatus(processedResults, request.Status);
            }

            var result = processedResults
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.ScoreableProperty)
                .Take(maxNumberOfResults)
                .Select(x => AdresMatchOsloItem.Create(x, _responseOptions.Value))
                .ToList();

            return Task.FromResult(new AddressMatchOsloCollection
            {
                Context = _responseOptions.Value.ContextUrlAddressMatch,
                AdresMatches = result,
                Warnings = warningLogger.Warnings
            });
        }

        private IReadOnlyList<AddressMatchScoreableItemV2> FilterByStatus(
            IReadOnlyList<AddressMatchScoreableItemV2> results,
            string status)
        {
            var hasAddressResults = results.Any(x => x.AdresStatus != null);

            if (hasAddressResults && Enum.TryParse<AdresStatus>(status, true, out var adresStatus))
            {
                return results.Where(x => x.AdresStatus == adresStatus).ToList();
            }

            if (!hasAddressResults && Enum.TryParse<StraatnaamStatus>(status, true, out var straatnaamStatus))
            {
                var streetNameStatus = MapStraatnaamStatus(straatnaamStatus);
                var streetNamesByPersistentLocalId = _latestQueries.GetAllLatestStreetNamesByPersistentLocalId();

                return results.Where(x =>
                {
                    if (x.Straatnaam?.ObjectId == null)
                        return false;

                    if (!int.TryParse(x.Straatnaam.ObjectId, out var persistentLocalId))
                        return false;

                    return streetNamesByPersistentLocalId.TryGetValue(persistentLocalId, out var streetName)
                           && streetName.Status == streetNameStatus;
                }).ToList();
            }

            return results;
        }

        private static StreetNameStatus MapStraatnaamStatus(StraatnaamStatus straatnaamStatus)
        {
            return straatnaamStatus switch
            {
                StraatnaamStatus.Voorgesteld => StreetNameStatus.Proposed,
                StraatnaamStatus.InGebruik => StreetNameStatus.Current,
                StraatnaamStatus.Gehistoreerd => StreetNameStatus.Retired,
                StraatnaamStatus.Afgekeurd => StreetNameStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(straatnaamStatus), straatnaamStatus, null)
            };
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
