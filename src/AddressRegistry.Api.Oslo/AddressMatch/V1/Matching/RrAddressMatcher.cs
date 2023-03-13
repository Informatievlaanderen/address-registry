namespace AddressRegistry.Api.Oslo.AddressMatch.V1.Matching
{
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Projections.Legacy.AddressDetail;

    internal class RrAddressMatcher<TResult> : MatcherBase<AddressMatchBuilder, TResult>
        where TResult : IScoreable
    {
        private readonly IKadRrService _kadRrService;
        private readonly IMapper<AddressDetailItem, TResult> _mapper;
        private readonly int _maxNumberOfResults;
        private readonly IWarningLogger _warnings;

        public RrAddressMatcher(
            IKadRrService kadRrService,
            IMapper<AddressDetailItem, TResult> mapper,
            int maxNumberOfResults,
            IWarningLogger warnings)
        {
            _kadRrService = kadRrService;
            _mapper = mapper;
            _maxNumberOfResults = maxNumberOfResults;
            _warnings = warnings;
        }

        protected override AddressMatchBuilder? DoMatchInternal(AddressMatchBuilder? builder)
        {
            if (string.IsNullOrEmpty(builder?.Query.HouseNumber) || string.IsNullOrEmpty(builder?.Query.RrStreetCode))
                return builder;

            var rrAddresses = _kadRrService
                .GetAddressesBy(
                    builder.Query.HouseNumber,
                    builder.Query.Index,
                    builder.Query.RrStreetCode,
                    builder.Query.PostalCode)
                .ToList();

            if (!rrAddresses.Any())
                return builder;

            builder.AddRrAddresses(rrAddresses);

            return builder;
        }

        protected override bool? ShouldProceed(AddressMatchBuilder? builder)
            => !IsValidMatch(builder);

        protected override bool? IsValidMatch(AddressMatchBuilder? builder)
            => builder?.AllAddresses().Any();

        public override IReadOnlyList<TResult>? BuildResults(AddressMatchBuilder? builder)
        {
            var results = builder?
                .AllAddresses()
                .Take(_maxNumberOfResults)
                .Select(_mapper.Map)
                .ToList();

            results?.ForEach(s => s.Score = 100);

            return results;
        }
    }
}
