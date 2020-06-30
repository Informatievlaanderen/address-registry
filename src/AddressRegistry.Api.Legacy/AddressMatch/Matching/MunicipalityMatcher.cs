namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using Projections.Syndication.Municipality;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class MunicipalityMatcher<TResult> : ScoreableObjectMatcherBase<AddressMatchBuilder, TResult>
        where TResult : class, IScoreable
    {
        private readonly ManualAddressMatchConfig _config;
        private readonly ILatestQueries _latestQueries;
        private readonly IMapper<MunicipalityLatestItem, TResult> _mapper;
        private readonly IWarningLogger _warnings;

        public MunicipalityMatcher(
            ILatestQueries latestQueries,
            ManualAddressMatchConfig config,
            IMapper<MunicipalityLatestItem, TResult> mapper,
            IWarningLogger warnings)
        {
            _latestQueries = latestQueries;
            _config = config;
            _mapper = mapper;
            _warnings = warnings;
        }

        protected override AddressMatchBuilder? DoMatchInternal(AddressMatchBuilder? results)
        {
            var municipalities = _latestQueries
                .GetAllLatestMunicipalities()
                .ToList();

            var municipalitiesByName = FilterByName(municipalities, results);

            var municipalitiesByNisCode = FilterByNisCode(municipalities, results);

            results?.AddMunicipalities(municipalitiesByName.Union(municipalitiesByNisCode));

            if (results.Count() > 1)
                _warnings.AddWarning("9", "Geen overeenkomst tussen 'Niscode' en 'Gemeentenaam'.");

            if (!string.IsNullOrEmpty(results?.Query.PostalCode))
                results
                    .AddMunicipalitiesByPostalCode(
                        FilterByPostalCode(municipalities, results),
                        results.Query.PostalCode);

            if (string.IsNullOrEmpty(results?.Query.MunicipalityName) || results.Any())
                return results;

            var municipalitiesByPostalCode = FilterByPartOfMunicipality(municipalities, results)
                .GroupBy(municipalityPostCode => municipalityPostCode.Item1);

            foreach (var municipalitiesGroup in municipalitiesByPostalCode)
                results.AddMunicipalitiesByPostalCode(
                    municipalitiesGroup
                        .Select(municipalityPostCode => municipalityPostCode.Item2),
                    municipalitiesGroup.Key);

            if (results.Any())
                return results;

            var municipalitiesByFuzzyMatch = municipalities
                .Where(g => g
                    .DefaultName.Value
                    .EqFuzzyMatch(results.Query.MunicipalityName, _config.FuzzyMatchThreshold));

            results.AddMunicipalities(municipalitiesByFuzzyMatch);

            return results;
        }

        protected override bool? ShouldProceed(AddressMatchBuilder? matchResult)
            => IsValidMatch(matchResult);

        protected override bool? IsValidMatch(AddressMatchBuilder? matchResult)
            => matchResult.Any();

        protected override IReadOnlyList<TResult> BuildResultsInternal(AddressMatchBuilder? results)
            => results
                .Select(g => g.Municipality)
                .Select(_mapper.Map)
                .ToList();

        private IEnumerable<MunicipalityLatestItem> FilterByName(
            IEnumerable<MunicipalityLatestItem> municipalities,
            AddressMatchBuilder? results)
        {
            if (string.IsNullOrWhiteSpace(results?.Query.MunicipalityName))
                return new List<MunicipalityLatestItem>();

            var municipalitiesByName = municipalities
                .Where(g => g.DefaultName.Value.EqIgnoreCase(results?.Query.MunicipalityName))
                .ToList();

            if (!string.IsNullOrEmpty(results?.Query.MunicipalityName) && !municipalitiesByName.Any())
                _warnings.AddWarning("6", "Onbekende 'Gemeentenaam'.");

            return municipalitiesByName;
        }

        private IEnumerable<MunicipalityLatestItem> FilterByNisCode(
            IEnumerable<MunicipalityLatestItem> municipalities,
            AddressMatchBuilder? results)
        {
            if (string.IsNullOrWhiteSpace(results?.Query.NisCode))
                return new List<MunicipalityLatestItem>();

            var municipalitiesByNisCode = municipalities
                .Where(g => !string.IsNullOrWhiteSpace(g.NisCode) && g.NisCode.Equals(results?.Query.NisCode))
                .ToList();

            if (!string.IsNullOrEmpty(results?.Query.NisCode) && !municipalitiesByNisCode.Any())
                _warnings.AddWarning("5", "Onbekende 'Niscode'.");

            return municipalitiesByNisCode;
        }

        private IEnumerable<MunicipalityLatestItem> FilterByPostalCode(
            IEnumerable<MunicipalityLatestItem> municipalities,
            AddressMatchBuilder results)
        {
            if (string.IsNullOrWhiteSpace(results?.Query.PostalCode))
                return new List<MunicipalityLatestItem>();

            var postalInfo = _latestQueries
                .GetAllPostalInfo()
                .FirstOrDefault(x => x.PostalCode == results.Query.PostalCode);

            if (postalInfo != null)
            {
                if (results.Any() && !results.ContainsNisCode(postalInfo.NisCode))
                    _warnings.AddWarning("8", "Geen overeenkomst tussen 'Postcode' en 'Gemeentenaam'/'Niscode'.");

                return string.IsNullOrEmpty(postalInfo.NisCode)
                    ? new List<MunicipalityLatestItem>()
                    : municipalities.Where(g => g.NisCode != null && g.NisCode.Contains(postalInfo.NisCode));
            }

            _warnings.AddWarning("4", "Onbekende 'Postcode'.");
            return new MunicipalityLatestItem[] { };
        }

        private IEnumerable<Tuple<string, MunicipalityLatestItem>> FilterByPartOfMunicipality(
            IEnumerable<MunicipalityLatestItem> municipalities,
            AddressMatchBuilder results)
            => _latestQueries
                .GetAllPostalInfo()
                .Where(postalInfo => postalInfo
                    .PostalNames
                    .Any(postalName =>
                        postalName.PostalName != null &&
                        postalName.PostalName.EqIgnoreCaseAndDiacritics(results.Query.MunicipalityName)))
                .Select(postalInfo => new
                {
                    PostalCode = postalInfo.PostalCode,
                    MissingNisCode = results.ContainsNisCode(postalInfo.NisCode)
                        ? null
                        : postalInfo.NisCode,
                })
                .SelectMany(x => municipalities
                    .Where(g => g.NisCode == x.MissingNisCode)
                    .Select(g => new Tuple<string, MunicipalityLatestItem>(x.PostalCode, g)));
    }
}
