namespace AddressRegistry.Api.BackOffice.IntegrationTests
{
    using System.Net;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class AddressControllerProposeTests
    {
        private const string Uri = "v2/adressen/acties/voorstellen";

        [Fact(Skip = "Does not use in-memory database.")]
        public async Task AddParentAddress_ShouldRespondNoContent()
        {
            var application = new WebApplicationFactory<Program>();

            var client = application.CreateClient();

            var response = await client.PostAsync(Uri, JsonContent.Create(new
            {
                postInfoId = "https://data.vlaanderen.be/id/postinfo/9000",
                straatNaamId = "https://data.vlaanderen.be/id/straatnaam/45041",
                huisNummer = "11",
                busNummer = ""
            }));

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.Contains("ETag");
        }
    }
}

