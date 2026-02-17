namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Projections.AddressMatch.AddressDetailV2WithParent;

    internal sealed class AddressMatcher<TResult> : ScoreableObjectMatcherBase<AddressMatchBuilder, TResult>
        where TResult : class, IScoreable
    {
        private readonly ILatestQueries _latestQueries;
        private readonly IMapper<AddressDetailItemV2WithParent, TResult> _mapper;
        private readonly Sanitizer _sanitizer;

        public AddressMatcher(
            ILatestQueries latestQueries,
            IMapper<AddressDetailItemV2WithParent, TResult> mapper,
            IWarningLogger warnings)
        {
            _latestQueries = latestQueries;
            _mapper = mapper;
            _sanitizer = new Sanitizer { Warnings = warnings };
        }

        protected override AddressMatchBuilder? DoMatchInternal(AddressMatchBuilder? results)
        {
            var houseNumberWithSubaddresses = new List<HouseNumberWithSubaddress>();

            houseNumberWithSubaddresses.AddRange(
                _sanitizer.Sanitize(
                    results?.Query.StreetName,
                    results?.Query.HouseNumber,
                    null));

            if (!string.IsNullOrEmpty(results?.Query.BoxNumber))
            {
                houseNumberWithSubaddresses.Insert(0,
                    new HouseNumberWithSubaddress(
                        results.Query.HouseNumber,
                        results.Query.BoxNumber,
                        null));
            }

            if (results == null)
            {
                return results;
            }

            var streetNameMatches = 0;
            foreach (var municipalityWrapper in results)
            {
                foreach (var streetNameWrapper in municipalityWrapper)
                {
                    foreach (var houseNumberWithSubaddress in houseNumberWithSubaddresses)
                    {
                        streetNameWrapper.AddAddresses(
                            _latestQueries
                                .GetLatestAddressesBy(
                                    streetNameWrapper.StreetName.PersistentLocalId,
                                    houseNumberWithSubaddress.HouseNumber,
                                    houseNumberWithSubaddress.BoxNumber,
                                    results.Query.AddressStatus)
                                .ToList());
                    }
                }

                streetNameMatches++;
                if (streetNameMatches > 10 && results.AllAddresses().Count() >= 10)
                {
                    return results;
                }
            }

            return results;
        }

        protected override bool? IsValidMatch(AddressMatchBuilder? matchResult)
            => matchResult?.AllAddresses().Any();

        //adresmatcher is the latest, so should never proceed
        protected override bool? ShouldProceed(AddressMatchBuilder? matchResult)
            => false;

        protected override IReadOnlyList<TResult>? BuildResultsInternal(AddressMatchBuilder? results)
            => results?
                .AllAddresses()
                .Select(_mapper.Map)
                .ToList();
    }
}
