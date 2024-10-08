namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using System.Collections.Generic;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using Projections.AddressMatch.AddressDetailV2WithParent;

    /// <summary>
    /// Implements an algorithm for matching AdresMatchQueryComponents to Gemeentes, Straatnamen or Adressen and scoring the results.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class AddressMatchMatchingAlgorithm<TResult> : MatchingAlgorithm<AddressMatchBuilder, TResult>
        where TResult : class, IScoreable
    {
        public AddressMatchMatchingAlgorithm(
            ManualAddressMatchConfig config,
            ILatestQueries latestQueries,
            IMapper<MunicipalityLatestItem, TResult> municipalityMapper,
            IMapper<StreetNameLatestItem, TResult> streetNameMapper,
            IMapper<AddressDetailItemV2WithParent, TResult> addressMapper,
            int maxNumberOfResults,
            IWarningLogger warnings) :
            base(
                new MunicipalityMatcher<TResult>(latestQueries, config, municipalityMapper, warnings),
                new StreetNameMatcher<TResult>(latestQueries, config, streetNameMapper, warnings),
                new AddressMatcher<TResult>(latestQueries, addressMapper, warnings)) { }

        public override IReadOnlyList<TResult> Process(AddressMatchBuilder seed)
            => base.Process(seed) ?? new List<TResult>();
    }
}
