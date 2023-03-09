namespace AddressRegistry.Api.Oslo.AddressMatch.V1.Matching
{
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Projections.Legacy.AddressDetail;

    internal class AddressMatcher<TResult> : ScoreableObjectMatcherBase<AddressMatchBuilder, TResult>
        where TResult : class, IScoreable
    {
        private readonly ILatestQueries _latestQueries;
        private readonly IMapper<AddressDetailItem, TResult> _mapper;
        private readonly Sanitizer _sanitizer;

        public AddressMatcher(
            ILatestQueries latestQueries,
            IMapper<AddressDetailItem, TResult> mapper,
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
                    results?.Query.Index));

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
                                    houseNumberWithSubaddress.BoxNumber)
                                .ToList());
                    }
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
