namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.Search
{
    using System.Collections.Generic;
    using AddressRegistry.Consumer.Read.StreetName.Projections.Elastic;
    using AddressRegistry.Infrastructure.Elastic;

    public sealed class StreetNameSearchResult
    {
        public IReadOnlyCollection<StreetNameSearchDocument> StreetNames { get; }
        public long Total { get; }
        public Language? Language { get; }

        public StreetNameSearchResult(IReadOnlyCollection<StreetNameSearchDocument> streetNames, long total, Language? language = null)
        {
            StreetNames = streetNames;
            Total = total;
            Language = language;
        }

        public static StreetNameSearchResult Empty => new StreetNameSearchResult(new List<StreetNameSearchDocument>(), 0);
    }
}
