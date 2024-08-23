namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Linq;
    using Projections.Elastic.AddressSearch;

    public sealed class AddressSearchResult
    {
        public IQueryable<AddressSearchDocument> Addresses { get; }
        public long Total { get; }

        public AddressSearchResult(IQueryable<AddressSearchDocument> addresses, long total)
        {
            Addresses = addresses;
            Total = total;
        }
    }
}
