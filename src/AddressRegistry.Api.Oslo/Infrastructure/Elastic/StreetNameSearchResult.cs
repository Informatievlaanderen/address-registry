namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Collections.Generic;
    using AddressRegistry.Infrastructure.Elastic;
    using Consumer.Read.StreetName.Projections.Elastic;

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
    }
}
