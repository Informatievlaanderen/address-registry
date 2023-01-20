namespace AddressRegistry.Api.BackOffice.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("/v2/adressen/acties/voorstellen", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/goedkeuren", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/afkeuren", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/opheffen", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/verwijderen", "dv_ar_adres_beheer dv_ar_adres_uitzonderingen")]
        [InlineData("/v2/adressen/1/acties/regulariseren", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/deregulariseren", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/adrespositie", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/postcode", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/huisnummer", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/busnummer", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/goedkeuring", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/afkeuring", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/opheffing", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/regularisatie", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/deregularisatie", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/wijzigen/adrespositie", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/wijzigen/postcode", "dv_ar_adres_beheer dv_ar_adres_uitzonderingen")]
        public async Task ReturnsSuccess(string endpoint, string requiredScopes)
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(requiredScopes));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/adressen/acties/voorstellen")]
        [InlineData("/v2/adressen/1/acties/goedkeuren")]
        [InlineData("/v2/adressen/1/acties/afkeuren")]
        [InlineData("/v2/adressen/1/acties/opheffen")]
        [InlineData("/v2/adressen/1/acties/verwijderen")]
        [InlineData("/v2/adressen/1/acties/regulariseren")]
        [InlineData("/v2/adressen/1/acties/deregulariseren")]
        [InlineData("/v2/adressen/1/acties/corrigeren/adrespositie")]
        [InlineData("/v2/adressen/1/acties/corrigeren/postcode")]
        [InlineData("/v2/adressen/1/acties/corrigeren/huisnummer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/busnummer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/goedkeuring")]
        [InlineData("/v2/adressen/1/acties/corrigeren/afkeuring")]
        [InlineData("/v2/adressen/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/adressen/1/acties/corrigeren/regularisatie")]
        [InlineData("/v2/adressen/1/acties/corrigeren/deregularisatie")]
        [InlineData("/v2/adressen/1/acties/wijzigen/adrespositie")]
        [InlineData("/v2/adressen/1/acties/wijzigen/postcode")]
        public async Task ReturnsUnauthorized(string endpoint)
        {
            var client = _fixture.TestServer.CreateClient();

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/adressen/acties/voorstellen")]
        [InlineData("/v2/adressen/1/acties/goedkeuren")]
        [InlineData("/v2/adressen/1/acties/afkeuren")]
        [InlineData("/v2/adressen/1/acties/opheffen")]
        [InlineData("/v2/adressen/1/acties/verwijderen")]
        [InlineData("/v2/adressen/1/acties/verwijderen", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/verwijderen", " dv_ar_adres_uitzonderingen")]
        [InlineData("/v2/adressen/1/acties/regulariseren")]
        [InlineData("/v2/adressen/1/acties/deregulariseren")]
        [InlineData("/v2/adressen/1/acties/corrigeren/adrespositie")]
        [InlineData("/v2/adressen/1/acties/corrigeren/postcode")]
        [InlineData("/v2/adressen/1/acties/corrigeren/huisnummer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/busnummer")]
        [InlineData("/v2/adressen/1/acties/corrigeren/goedkeuring")]
        [InlineData("/v2/adressen/1/acties/corrigeren/afkeuring")]
        [InlineData("/v2/adressen/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/adressen/1/acties/corrigeren/regularisatie")]
        [InlineData("/v2/adressen/1/acties/corrigeren/deregularisatie")]
        [InlineData("/v2/adressen/1/acties/wijzigen/adrespositie")]
        [InlineData("/v2/adressen/1/acties/wijzigen/postcode")]
        [InlineData("/v2/adressen/1/acties/wijzigen/postcode", "dv_ar_adres_beheer")]
        [InlineData("/v2/adressen/1/acties/wijzigen/postcode", " dv_ar_adres_uitzonderingen")]
        public async Task ReturnsForbidden(string endpoint, string scope = "")
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(scope));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
