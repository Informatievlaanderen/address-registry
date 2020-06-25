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
        where TResult : class, IScoreable
    {
        public AddressMatchMatchingAlgorithm(
            IKadRrService kadRrService,
            ManualAddressMatchConfig config,
            ILatestQueries latestQueries,
            IMapper<MunicipalityLatestItem, TResult> municipalityMapper,
            IMapper<StreetNameLatestItem, TResult> streetNameMapper,
            IMapper<AddressDetailItem, TResult> addressMapper,
            int maxNumberOfResults,
            IWarningLogger warnings) :
            base(
                new RrAddressMatcher<TResult>(kadRrService, addressMapper, maxNumberOfResults, warnings),
                new MunicipalityMatcher<TResult>(latestQueries, config, municipalityMapper, warnings),
                new StreetNameMatcher<TResult>(latestQueries, kadRrService, config, streetNameMapper, warnings),
                new AddressMatcher<TResult>(latestQueries, addressMapper, warnings)) { }

        public override IReadOnlyList<TResult> Process(AddressMatchBuilder seed)
            => base.Process(seed) ?? new List<TResult>();
    }
}
