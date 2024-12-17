namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.Search
{
    using System.Collections.Generic;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.Projections.Elastic.AddressSearch;

    public sealed class AddressSearchResult
    {
        public IReadOnlyCollection<AddressSearchDocument> Addresses { get; }
        public long Total { get; }
        public Language? Language { get; }

        public AddressSearchResult(IReadOnlyCollection<AddressSearchDocument> addresses, long total, Language? language = null)
        {
            Addresses = addresses;
            Total = total;
            Language = language;
        }

        public static AddressSearchResult Empty => new AddressSearchResult(new List<AddressSearchDocument>(), 0);
    }
}
