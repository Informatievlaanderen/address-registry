namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.Search
{
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.StreetName.Projections;

    public interface IAddressApiStreetNameElasticsearchClient
    {
        Task<StreetNameSearchResult> SearchStreetNames(
            string query,
            string? nisCode,
            StreetNameStatus? status,
            int size = 10);
    }
}
