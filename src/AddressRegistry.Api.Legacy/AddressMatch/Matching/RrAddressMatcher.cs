namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using System.Collections.Generic;
    using System.Linq;
    using Projections.Legacy.AddressDetail;

    internal class RrAddressMatcher<TResult> : MatcherBase<AddressMatchBuilder, TResult>
        where TResult : IScoreable
    {
        private readonly IKadRrService _kadRrService;
        private readonly IMapper<AddressDetailItem, TResult> _mapper;
        private readonly IWarningLogger _warmings;

        public RrAddressMatcher(IKadRrService kadRrService, IMapper<AddressDetailItem, TResult> mapper, IWarningLogger warnings)
        {
            _kadRrService = kadRrService;
            _mapper = mapper;
            _warmings = warnings;
        }

        public override IReadOnlyList<TResult> BuildResults(AddressMatchBuilder builder)
        {
            List<TResult> results = builder.AllAddresses().Select(_mapper.Map).ToList();
            results.ForEach(s => s.Score = 100);

            return results;

        }

        protected override AddressMatchBuilder DoMatchInternal(AddressMatchBuilder builder)
        {
            if (!string.IsNullOrEmpty(builder.Query.HouseNumber) && !string.IsNullOrEmpty(builder.Query.RrStreetCode))
            {
                IEnumerable<AddressDetailItem> rrAddresses = _kadRrService.GetAddressesBy(builder.Query.HouseNumber, builder.Query.Index, builder.Query.RrStreetCode, builder.Query.PostalCode).ToList();
                if (rrAddresses.Any())
                {
                    builder.AddRrAddresses(rrAddresses);
                    _warmings.AddWarning("21", "De adressen in het resultaat werden gevonden via een rechtstreekse link naar het opgegeven rijksregister adres.");
                }
            }
            return builder;
        }

        protected override bool IsValidMatch(AddressMatchBuilder builder)
        {
            return builder.AllAddresses().Any();
        }

        protected override bool ShouldProceed(AddressMatchBuilder builder)
        {
            return !IsValidMatch(builder);
        }
    }
}
