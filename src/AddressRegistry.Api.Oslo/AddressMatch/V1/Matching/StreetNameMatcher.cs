namespace AddressRegistry.Api.Oslo.AddressMatch.V1.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Projections.Syndication.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;

    internal class StreetNameMatcher<TResult> : ScoreableObjectMatcherBase<AddressMatchBuilder, TResult>
        where TResult : class, IScoreable
    {
        private readonly ILatestQueries _latestQueries;
        private readonly IKadRrService _streetNameService;
        private readonly ManualAddressMatchConfig _config;
        private readonly IMapper<StreetNameLatestItem, TResult> _mapper;
        private readonly IWarningLogger _warnings;

        public StreetNameMatcher(
            ILatestQueries latestQueries,
            IKadRrService streetNameService,
            ManualAddressMatchConfig config,
            IMapper<StreetNameLatestItem, TResult> mapper,
            IWarningLogger warnings)
        {
            _latestQueries = latestQueries;
            _streetNameService = streetNameService;
            _config = config;
            _mapper = mapper;
            _warnings = warnings;
        }

        protected override AddressMatchBuilder? DoMatchInternal(AddressMatchBuilder? results)
        {
            if (!string.IsNullOrEmpty(results?.Query.KadStreetNameCode))
                FindStreetNamesByKadStreetCode(results);

            if (!string.IsNullOrEmpty(results?.Query.RrStreetCode) && !string.IsNullOrEmpty(results.Query.PostalCode))
                FindStreetNamesByRrStreetCode(results);

            if (!string.IsNullOrEmpty(results?.Query.StreetName))
            {
                var municipalityWithStreetNames =
                    _latestQueries
                        .GetLatestStreetNamesBy(
                            results
                                .Select(g => g.Name)
                                .ToArray())
                        .Where(s => s.NisCode != null)
                        .GroupBy(s => s.NisCode!)
                        .ToDictionary(g => g.Key, g => g.ToList());

                FindStreetNamesByName(results, municipalityWithStreetNames);

                var isValidStreetnameMatch = IsValidMatch(results);
                if (isValidStreetnameMatch.HasValue && !isValidStreetnameMatch.Value)
                    FindStreetNamesByNameToggleAbreviations(results, municipalityWithStreetNames);

                isValidStreetnameMatch = IsValidMatch(results);
                if (isValidStreetnameMatch.HasValue && !isValidStreetnameMatch.Value)
                    FindStreetNamesByFuzzyMatch(results, municipalityWithStreetNames);

                isValidStreetnameMatch = IsValidMatch(results);
                if (isValidStreetnameMatch.HasValue && !isValidStreetnameMatch.Value)
                    FindStreetNamesByFuzzyMatchToggleAbreviations(results, municipalityWithStreetNames);

                isValidStreetnameMatch = IsValidMatch(results);
                if (isValidStreetnameMatch.HasValue && !isValidStreetnameMatch.Value && results.Query.StreetName.Length > 3)
                    FindStreetNamesByStreetNameContainsInput(results, municipalityWithStreetNames);

                isValidStreetnameMatch = IsValidMatch(results);
                if (isValidStreetnameMatch.HasValue && !isValidStreetnameMatch.Value && results.Query.StreetName.Length > 3)
                    FindStreetNamesByInputContainsStreetName(results, municipalityWithStreetNames);
            }

            var validMatch = IsValidMatch(results);
            if (validMatch.HasValue && !validMatch.Value)
            {
                _warnings.AddWarning("10", "'Straatnaam' niet interpreteerbaar.");
            }
            else if (results?.AllStreetNames().Count() > _config.MaxStreetNamesThreshold)
            {
                _warnings.AddWarning("100", "Uw zoekopdracht is te generiek");
                results.ClearAllStreetNames();
            }

            return results;
        }

        protected override bool? IsValidMatch(AddressMatchBuilder? matchResult)
            => matchResult?.AllStreetNames().Any();

        protected override bool? ShouldProceed(AddressMatchBuilder? matchResult)
            => IsValidMatch(matchResult);

        protected override IReadOnlyList<TResult>? BuildResultsInternal(AddressMatchBuilder? results)
            => results?
                .AllStreetNames()
                .Select(w => _mapper.Map(w.StreetName))
                .ToList();

        private void FindStreetNamesByKadStreetCode(AddressMatchBuilder results)
        {
            if (string.IsNullOrWhiteSpace(results?.Query.KadStreetNameCode))
                return;

            foreach (var municipalityWrapper in results)
                municipalityWrapper
                    .AddStreetNames(
                        _streetNameService
                            .GetStreetNamesByKadStreet(
                                results.Query.KadStreetNameCode,
                                municipalityWrapper.NisCode));

            var searchName = results
                ?.Query
                ?.StreetName
                ?.RemoveDiacritics();
        }

        private void FindStreetNamesByRrStreetCode(AddressMatchBuilder results)
        {
            var streetName = _streetNameService
                .GetStreetNameByRrStreet(
                    results.Query.RrStreetCode,
                    results.Query.PostalCode);

            if (streetName != null)
                results
                    .Where(g => g.PostalCode == results.Query.PostalCode)
                    .ToList()
                    .ForEach(g => g.AddStreetName(streetName));

            var searchName = results
                ?.Query
                ?.StreetName
                ?.RemoveDiacritics();
        }

        private static void FindStreetNamesByName(AddressMatchBuilder results, IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
            => FindStreetNamesBy(
                results,
                municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqIgnoreCase(requestedStreetName));

        private static void FindStreetNamesByNameToggleAbreviations(AddressMatchBuilder results, IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
            => FindStreetNamesBy(
                results,
                municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqIgnoreCase(requestedStreetName.ToggleAbbreviations()));

        private void FindStreetNamesByFuzzyMatch(AddressMatchBuilder results, IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
            => FindStreetNamesBy(
                results,
                municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqFuzzyMatch(requestedStreetName, _config.FuzzyMatchThreshold));

        private void FindStreetNamesByFuzzyMatchToggleAbreviations(AddressMatchBuilder results, IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
            => FindStreetNamesBy(
                results,
                municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqFuzzyMatchToggleAbreviations(requestedStreetName, _config.FuzzyMatchThreshold));

        private static void FindStreetNamesByStreetNameContainsInput(AddressMatchBuilder results, IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
            => FindStreetNamesBy(
                results,
                municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.ContainsIgnoreCase(requestedStreetName));

        private static void FindStreetNamesByInputContainsStreetName(AddressMatchBuilder results, IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
            => FindStreetNamesBy(
                results,
                municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => requestedStreetName.ContainsIgnoreCase(exisitingStreetName));

        private static void FindStreetNamesBy(
            AddressMatchBuilder results,
            IReadOnlyDictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames,
            Func<string, string, bool> comparer)
        {
            foreach (var municipalityWrapper in results)
            {
                if (string.IsNullOrWhiteSpace(municipalityWrapper.NisCode))
                    continue;

                if (!municipalitiesWithStreetNames.ContainsKey(municipalityWrapper.NisCode))
                    continue;

                var municipalityWithStreetNames = municipalitiesWithStreetNames[municipalityWrapper.NisCode];

                var searchName = results
                    ?.Query
                    ?.StreetName;

                if (string.IsNullOrWhiteSpace(searchName))
                    continue;

                var matchingStreetName = municipalityWithStreetNames
                    .Where(s =>
                        !string.IsNullOrWhiteSpace(s.NameDutch) && comparer(s.NameDutch, searchName) ||
                        !string.IsNullOrWhiteSpace(s.NameFrench) && comparer(s.NameFrench, searchName) ||
                        !string.IsNullOrWhiteSpace(s.NameGerman) && comparer(s.NameGerman, searchName) ||
                        !string.IsNullOrWhiteSpace(s.NameEnglish) && comparer(s.NameEnglish, searchName))
                    .ToList();

                municipalityWrapper.AddStreetNames(matchingStreetName);

                if (matchingStreetName.Any())
                    continue;

                searchName = searchName.RemoveDiacritics()!;

                municipalityWrapper.AddStreetNames(
                    municipalityWithStreetNames
                        .Where(s =>
                            !string.IsNullOrWhiteSpace(s.NameDutchSearch) && comparer(s.NameDutchSearch, searchName) ||
                            !string.IsNullOrWhiteSpace(s.NameFrenchSearch) && comparer(s.NameFrenchSearch, searchName) ||
                            !string.IsNullOrWhiteSpace(s.NameGermanSearch) && comparer(s.NameGermanSearch, searchName) ||
                            !string.IsNullOrWhiteSpace(s.NameEnglishSearch) && comparer(s.NameEnglishSearch, searchName))
                        .ToList());

            }
        }
    }
}
