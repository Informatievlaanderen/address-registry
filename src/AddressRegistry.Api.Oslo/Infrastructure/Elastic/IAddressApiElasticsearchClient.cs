namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StreetName;

    public interface IAddressApiElasticsearchClient
    {
        Task<AddressSearchResult> SearchAddresses(
            string addressQuery,
            string? municipalityName,
            AddressStatus? status,
            int size = 10);

        Task<AddressSearchResult> ListAddresses(
            string? streetNameId,
            string? streetName,
            string? homonymAddition,
            string? houseNumber,
            string? boxNumber,
            string? postalCode,
            string? nisCode,
            string? municipalityName,
            string? status,
            int? from,
            int? size);
    }
}
