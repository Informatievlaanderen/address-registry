namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Threading.Tasks;

    public interface IAddressApiStreetNameElasticsearchClient
    {
        Task<StreetNameSearchResult> SearchStreetNames(
            string query,
            string? municipalityOrPostalName,
            int size = 10);
    }
}
