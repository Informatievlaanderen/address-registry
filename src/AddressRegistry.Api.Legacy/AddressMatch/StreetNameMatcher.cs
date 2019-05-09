namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Projections.Syndication.StreetName;

    internal class StreetNameMatcher<TResult> : ScoreableObjectMatcherBase<AddressMatchBuilder, TResult>
        where TResult : IScoreable
    {
        private enum StraatnaamMatchLevel
        {
            KadStraatCode = 1,
            RrStraatCode = 2,
            ExactMatch = 3,
            ExactMatchReplaceAbbreviations = 4,
            FuzzyMatch = 5,
            FuzzyMatchReplaceAbbreviations = 6,
            MatchStraatnaamContainsInput = 7,
            MatchInputContainsStraatnaam = 8,
            NoMatch = 9,
        }

        private readonly ILatestQueries _latestQueries;
        private readonly IKadRrService _streetNameService;
        private readonly ManualAddressMatchConfig _config;
        private readonly IMapper<StreetNameLatestItem, TResult> _mapper;
        private readonly IWarningLogger _warnings;
        //private readonly ITelemetry _telemetry;

        public StreetNameMatcher(
            ILatestQueries latestQueries,
            IKadRrService streetNameService,
            ManualAddressMatchConfig config,
            IMapper<StreetNameLatestItem, TResult> mapper,
            IWarningLogger warnings)
            //ITelemetry telemetry) TODO: Datadog
        {
            _latestQueries = latestQueries;
            _streetNameService = streetNameService;
            _config = config;
            _mapper = mapper;
            _warnings = warnings;
            //_telemetry = telemetry;
        }

        protected override IReadOnlyList<TResult> BuildResultsInternal(AddressMatchBuilder results)
        {
            return results.AllStreetNames().Select(w => _mapper.Map(w.StreetName)).ToList();
        }

        protected override AddressMatchBuilder DoMatchInternal(AddressMatchBuilder results)
        {
            if (!string.IsNullOrEmpty(results.Query.KadStreetNameCode))
                FindStreetNamesByKadStreetCode(results);

            if (!string.IsNullOrEmpty(results.Query.RrStreetCode) && !string.IsNullOrEmpty(results.Query.PostalCode))
                FindStreetNamesByRrStreetCode(results);

            if (!string.IsNullOrEmpty(results.Query.StreetName))
            {
                Dictionary<string, List<StreetNameLatestItem>> municipalityWithStreetNames =
                    _latestQueries.GetLatestStreetNamesBy(results.Select(g => g.Name).ToArray()).GroupBy(s => s.NisCode).ToDictionary(g => g.Key, g => g.ToList());

                FindStreetNamesByName(results, municipalityWithStreetNames);

                if (!IsValidMatch(results))
                    FindStreetNamesByNameToggleAbreviations(results, municipalityWithStreetNames);
                if (!IsValidMatch(results))
                    FindStratenByFuzzyMatch(results, municipalityWithStreetNames);
                if (!IsValidMatch(results))
                    FindStratenByFuzzyMatchToggleAbreviations(results, municipalityWithStreetNames);
                if (!IsValidMatch(results) && results.Query.StreetName.Length > 3)
                    FindStratenByStraatnaamContainsInput(results, municipalityWithStreetNames);
                if (!IsValidMatch(results) && results.Query.StreetName.Length > 3)
                    FindStratenByInputContainsStraatnaam(results, municipalityWithStreetNames);
            }

            if (!IsValidMatch(results))
            {
                _warnings.AddWarning("10", "'Straatnaam' niet interpreteerbaar.");
                //foreach (var gemeente in results)
                //    _telemetry.TrackStraatnaamMatch(gemeente.Naam, results.Query.Straatnaam, results.Query.KadStraatcode, results.Query.RrStraatcode, 0, (int)StraatnaamMatchLevel.NoMatch);
            }
            else if (results.AllStreetNames().Count() > _config.MaxStreetNamesThreshold)
            {
                _warnings.AddWarning("100", "Uw zoekopdracht is te generiek");
                results.ClearAllStreetNames();
            }


            return results;
        }

        protected override bool IsValidMatch(AddressMatchBuilder matchResult)
        {
            return matchResult.AllStreetNames().Any();
        }

        protected override bool ShouldProceed(AddressMatchBuilder matchResult)
        {
            return IsValidMatch(matchResult);
        }

        private void FindStreetNamesByKadStreetCode(AddressMatchBuilder results)
        {
            foreach (var municipalityWrapper in results)
            {
                municipalityWrapper.AddStreetNames(_streetNameService.GetStreetNamesByKadStreet(results.Query.KadStreetNameCode, municipalityWrapper.NisCode));
                //_telemetry.TrackStraatnaamMatch(gemeente.Naam, results.Query.Straatnaam, results.Query.KadStraatcode, results.Query.RrStraatcode, gemeente.Count(), (int)StraatnaamMatchLevel.KadStraatCode);
            }
            if (!string.IsNullOrEmpty(results.Query.StreetName) && !results.AllStreetNames().Any(w => w.StreetName.NameDutch.EqIgnoreCase(results.Query.StreetName)))
                _warnings.AddWarning("7", "Geen overeenkomst tussen 'KadStraatcode' en 'Straatnaam'.");
        }

        private void FindStreetNamesByRrStreetCode(AddressMatchBuilder results)
        {
            var streetName = _streetNameService.GetStreetNameByRrStreet(results.Query.RrStreetCode, results.Query.PostalCode);
            if (streetName != null)
            {
                results.Where(g => g.PostalCode == results.Query.PostalCode).ToList().ForEach(g => g.AddStreetName(streetName));
                //_telemetry.TrackStraatnaamMatch(results.Where(g => g.Postcode == results.Query.Postcode).FirstOrDefault()?.Naam, results.Query.Straatnaam, results.Query.KadStraatcode, results.Query.RrStraatcode, 1, (int)StraatnaamMatchLevel.RrStraatCode);
            }

            if (!string.IsNullOrEmpty(results.Query.StreetName) && !results.AllStreetNames().Any(w => w.StreetName.NameDutch.EqIgnoreCase(results.Query.StreetName)))
                _warnings.AddWarning("7", "Geen overeenkomst tussen 'RrStraatcode' en 'Straatnaam'.");

        }

        private void FindStreetNamesByName(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
        {
            FindStratenBy(results, municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqIgnoreCase(requestedStreetName), StraatnaamMatchLevel.ExactMatch);
        }

        private void FindStreetNamesByNameToggleAbreviations(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
        {
            FindStratenBy(results, municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqIgnoreCase(requestedStreetName.ToggleAbbreviations()), StraatnaamMatchLevel.ExactMatchReplaceAbbreviations);
        }

        private void FindStratenByFuzzyMatch(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
        {
            FindStratenBy(results, municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqFuzzyMatch(requestedStreetName, _config.FuzzyMatchThreshold), StraatnaamMatchLevel.FuzzyMatch);
        }

        private void FindStratenByFuzzyMatchToggleAbreviations(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
        {
            FindStratenBy(results, municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.EqFuzzyMatchToggleAbreviations(requestedStreetName, _config.FuzzyMatchThreshold), StraatnaamMatchLevel.FuzzyMatchReplaceAbbreviations);
        }

        private void FindStratenByStraatnaamContainsInput(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
        {
            FindStratenBy(results, municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => exisitingStreetName.ContainsIgnoreCase(requestedStreetName), StraatnaamMatchLevel.MatchStraatnaamContainsInput);
        }

        private void FindStratenByInputContainsStraatnaam(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames)
        {
            FindStratenBy(results, municipalitiesWithStreetNames,
                (exisitingStreetName, requestedStreetName) => requestedStreetName.ContainsIgnoreCase(exisitingStreetName), StraatnaamMatchLevel.MatchInputContainsStraatnaam);
        }

        private void FindStratenBy(AddressMatchBuilder results, Dictionary<string, List<StreetNameLatestItem>> municipalitiesWithStreetNames, Func<string, string, bool> comparer, StraatnaamMatchLevel level)
        {
            foreach (var municipalityWrapper in results)
            {
                if (municipalitiesWithStreetNames.ContainsKey(municipalityWrapper.NisCode))
                {
                    var municipalityWithStreetNames = municipalitiesWithStreetNames[municipalityWrapper.NisCode];

                    var matchingStreetName = municipalityWithStreetNames.Where(s => comparer(s.NameDutch, results.Query.StreetName));
                    municipalityWrapper.AddStreetNames(matchingStreetName);

                    //if (matchingStreetName.Any())
                    //    _telemetry.TrackStraatnaamMatch(municipalityWrapper.Naam, results.Query.Straatnaam, results.Query.KadStraatcode, results.Query.RrStraatcode, matchingStreetName.Count(), (int)level);
                }
            }
        }
    }
}
