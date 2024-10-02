namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Linq;
    using AddressRegistry.Infrastructure.Elastic;
    using Projections.Elastic.AddressSearch;

    public sealed class AddressSearchResult
    {
        public IQueryable<AddressSearchDocument> Addresses { get; }
        public long Total { get; }
        public Language? Language { get; }

        public AddressSearchResult(IQueryable<AddressSearchDocument> addresses, long total, Language? language = null)
        {
            Addresses = addresses;
            Total = total;
            Language = language;
        }
    }
}
