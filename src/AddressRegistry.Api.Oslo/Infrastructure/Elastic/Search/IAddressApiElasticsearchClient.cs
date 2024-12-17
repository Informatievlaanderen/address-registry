namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.Search
{
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;

    public interface IAddressApiSearchElasticsearchClient
    {
        Task<AddressSearchResult> SearchAddresses(
            string addressQuery,
            string? nisCode,
            AddressStatus? status,
            int size = 10);
    }
}
