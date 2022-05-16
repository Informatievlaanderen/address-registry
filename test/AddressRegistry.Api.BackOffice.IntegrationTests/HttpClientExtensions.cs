namespace AddressRegistry.Api.BackOffice.IntegrationTests
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class HttpClientExtensions
    {
        public static async Task<T?> GetJsonAsync<T>(this HttpClient client, string? requestUri, CancellationToken cancellationToken = default)
        {
            var s = await client.GetStringAsync(requestUri, cancellationToken);
            var result = JsonConvert.DeserializeObject<T>(s, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return result;
        }

        public static void SetPaginationHeader(this HttpClient client, string headerName, int? offset, int? limit)
        {
            client.DefaultRequestHeaders.Remove(headerName);
            client.DefaultRequestHeaders.Add(headerName, $"{offset},{limit}");
        }
    }
}
