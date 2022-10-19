namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Newtonsoft.Json;

    public interface IPublicApiHttpProxy
    {
        Task<OsloResult> GetSnapshot(string PersistentLocalId, CancellationToken cancellationToken);
    }

    public sealed class PublicApiHttpProxy : IPublicApiHttpProxy
    {
        private readonly HttpClient _httpClient;

        public PublicApiHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OsloResult> GetSnapshot(string PersistentLocalId, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_httpClient.BaseAddress}/{PersistentLocalId}");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Accept", "application/ld+json");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var osloResult = JsonConvert.DeserializeObject<OsloResult>(jsonContent, new JsonSerializerSettings().ConfigureDefaultForApi());

            if (osloResult is null)
            {
                throw new JsonSerializationException();
            }

            osloResult.JsonContent = jsonContent;

            return osloResult;
        }
    }

    public class OsloResult
    {
        public OsloResult()
        { }

        public OsloIdentificator Identificator { get; set; }

        [JsonIgnore]
        public string JsonContent { get; set; }
    }

    public class OsloIdentificator : Identificator
    {
        public OsloIdentificator()
        : base(string.Empty, string.Empty, string.Empty)
        { }
    }
}
