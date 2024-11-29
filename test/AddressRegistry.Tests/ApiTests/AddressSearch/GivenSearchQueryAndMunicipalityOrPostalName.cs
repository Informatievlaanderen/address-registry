namespace AddressRegistry.Tests.ApiTests.AddressSearch
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Oslo.Address.Search;
    using Api.Oslo.Infrastructure.Elastic;
    using Api.Oslo.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Consumer.Read.StreetName.Projections.Elastic;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Elastic.AddressSearch;
    using StreetName;
    using Xunit;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public class GivenSearchQueryAndMunicipalityOrPostalName
    {
        private readonly AddressSearchHandler _sut;
        private readonly Mock<IAddressApiElasticsearchClient> _mockAddressSearchApi;
        private readonly Mock<IAddressApiStreetNameElasticsearchClient> _mockAddressStreetNameSearchApi;

        public GivenSearchQueryAndMunicipalityOrPostalName()
        {
            _mockAddressSearchApi = new Mock<IAddressApiElasticsearchClient>();
            _mockAddressStreetNameSearchApi = new Mock<IAddressApiStreetNameElasticsearchClient>();

            _mockAddressSearchApi.Setup(x => x.SearchAddresses(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AddressStatus?>(), It.IsAny<int>()))
                .ReturnsAsync(new AddressSearchResult(new List<AddressSearchDocument>(), 0));
            _mockAddressStreetNameSearchApi.Setup(x => x.SearchStreetNames(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StreetNameStatus?>(), It.IsAny<int>()))
                .ReturnsAsync(new StreetNameSearchResult(new List<StreetNameSearchDocument>(), 0));

            var mockResponseOptions = new Mock<IOptions<ResponseOptions>>();

            _sut = new AddressSearchHandler(_mockAddressSearchApi.Object, _mockAddressStreetNameSearchApi.Object, mockResponseOptions.Object, Mock.Of<IMunicipalityCache>());
        }

        [Theory]
        [InlineData("bla bla 1", null)]
        [InlineData("bla 2A", "gent")]
        [InlineData("foo 3 bar", "gent")]
        [InlineData("veldstraat 5 bus 1 gen", "gent")]
        [InlineData("veldstraat 5A bus 1_3 9000 get", "gent")]
        [InlineData("veldstraat 5 bus A, 9000 abc", "gent")]
        [InlineData("veldstraat 5 bus 1.2, 9000 def", "gent")]
        [InlineData("veldstraat 5 bte 1.2, 9000 foo", "gent")]
        [InlineData("veldstraat 5 boite 1.2, 9000 bar", "gent")]
        [InlineData("veldstraat 5 boîte 1.2, 9000 genter", "gent")]
        [InlineData("veldstraat 5 box 1.2, 9000 gents", "gent")]
        public async Task WithHouseNumber_ThenNoResults(
            string query,
            string municipalityNameOrPostalNameQuery)
        {
            var limit = 20;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityName = municipalityNameOrPostalNameQuery}),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchAddresses(query, municipalityNameOrPostalNameQuery, null, limit), Times.Once);
        }

        [Theory]
        [InlineData("loppem", "muni")]
        [InlineData("street", "postal")]
        public async Task WithOneWordAndMunicipalityOrPostalName1_ThenSearchStreetNames(string query, string municipalityOrPostalName)
        {
            var limit = 10;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityName = municipalityOrPostalName}),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressStreetNameSearchApi.Verify(x => x.SearchStreetNames(query, municipalityOrPostalName, null, limit), Times.Once);
        }

        [Theory]
        [InlineData("loppem", "zedel")]
        [InlineData("one two", "three")]
        [InlineData("one two three", "four")]
        public async Task WithMultipleWords_ThenSearchStreetNamesWithMunicipalityOrPostalName(
            string query,
            string municipalityOrPostalName)
        {
            var limit = 50;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityName = municipalityOrPostalName}),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressStreetNameSearchApi.Verify(x => x.SearchStreetNames(query, municipalityOrPostalName, null, limit), Times.Once);
        }
    }
}
