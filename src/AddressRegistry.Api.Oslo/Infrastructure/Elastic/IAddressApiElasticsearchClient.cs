namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAddressApiElasticsearchClient
    {
        Task<IEnumerable<StreetNameSearchResult>> SearchStreetNames(string query, int size = 10);

        Task<IEnumerable<StreetNameSearchResult>> SearchStreetNames(
            string[] streetNameQueries,
            string municipalityOrPostalName,
            bool mustBeInMunicipality,
            int size = 10);

        Task<AddressSearchResult> SearchAddresses(
            string streetNameQuery,
            string houseNumberQuery,
            string? boxNumberQuery,
            string? postalCodeQuery,
            string? municipalityOrPostalName,
            bool mustBeInMunicipality,
            int? size = 10);

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
