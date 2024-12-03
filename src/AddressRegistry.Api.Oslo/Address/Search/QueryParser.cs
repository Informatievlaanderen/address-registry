namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System.Linq;
    using System.Text.RegularExpressions;

    public class QueryParser
    {
        private static readonly Regex PostalCodeRegex = new(@"\b\d{4}\b", RegexOptions.Compiled);

        private readonly IPostalCache _postalCache;

        public QueryParser(IPostalCache postalCache)
        {
            _postalCache = postalCache;
        }

        public bool TryExtractNisCodeViaPostalCode(ref string query, out string? nisCode)
        {
            var matches = PostalCodeRegex.Matches(query);
            if (matches.Count != 1)
            {
                nisCode = null;
                return false;
            }

            var match = matches.Single();
            var matchingNisCode = _postalCache.GetNisCodeByPostalCode(match.Value);
            if (matchingNisCode is not null)
            {
                query = query.Replace(match.Value, string.Empty);
                nisCode = matchingNisCode;
                return true;
            }

            nisCode = null;
            return false;
        }
    }
}
