namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using System.Collections.Generic;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    /// <summary>
    /// Implements an algorithm for matching AdresMatchQueryComponents to Gemeentes, Straatnamen or Adressen and scoring the results.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class AddressMatchMatchingAlgorithm<TResult> : MatchingAlgorithm<AddressMatchBuilder, TResult>
        where TResult : IScoreable
    {
        //private readonly ITelemetry _telemetry; TODO:Datadog

        public AddressMatchMatchingAlgorithm(
            IKadRrService kadRrService,
            ManualAddressMatchConfig config,
            ILatestQueries latestQueries,
            IMapper<MunicipalityLatestItem, TResult> municipalityMapper,
            IMapper<StreetNameLatestItem, TResult> streetNameMapper,
            IMapper<AddressDetailItem, TResult> addressMapper,
            IWarningLogger warnings) :
            //ITelemetry telemetry) :
            base(new RrAddressMatcher<TResult>(kadRrService, addressMapper, warnings),
                new MunicipalityMatcher<TResult>(latestQueries, config, municipalityMapper, warnings),//, telemetry),
                new StreetNameMatcher<TResult>(latestQueries, kadRrService, config, streetNameMapper, warnings),//, telemetry),
                new AddressMatcher<TResult>(latestQueries, addressMapper, warnings))
        {
            //_telemetry = telemetry;
        }

        public override IReadOnlyList<TResult> Process(AddressMatchBuilder seed)
        {
            IReadOnlyList<TResult> results = base.Process(seed);

            if (results == null)
                return new List<TResult>();

            //_telemetry.TrackAdresMatch(seed.Query.MunicipalityName, seed.Query.NisCode, seed.Query.PostalCode, seed.Query.StreetName, seed.Query.KadStreetNameCode, seed.Query.RrStreetCode, seed.Query.HouseNumber, seed.Query.Index, results.Count);
            //if (results.Any())
            //    _telemetry.TrackHighestScore(results.First().Score);

            return results;
        }
    }
}
