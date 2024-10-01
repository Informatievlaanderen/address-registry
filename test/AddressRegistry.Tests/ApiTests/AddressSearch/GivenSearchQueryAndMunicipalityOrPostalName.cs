namespace AddressRegistry.Tests.ApiTests.AddressSearch
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Oslo.Address.Search;
    using Api.Oslo.Infrastructure.Elastic;
    using Api.Oslo.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Elastic.AddressSearch;
    using Xunit;

    public class GivenSearchQueryAndMunicipalityOrPostalName
    {
        private readonly AddressSearchHandler _sut;
        private readonly Mock<IAddressApiElasticsearchClient> _mockAddressSearchApi;

        public GivenSearchQueryAndMunicipalityOrPostalName()
        {
            _mockAddressSearchApi = new Mock<IAddressApiElasticsearchClient>();
            _mockAddressSearchApi.Setup(x => x.SearchAddresses(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new AddressSearchResult(new List<AddressSearchDocument>().AsQueryable(), 0));

            var mockResponseOptions = new Mock<IOptions<ResponseOptions>>();

            _sut = new AddressSearchHandler(_mockAddressSearchApi.Object, mockResponseOptions.Object);
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
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityOrPostalName = municipalityNameOrPostalNameQuery}),
                    new SortingHeader("fake", SortOrder.Ascending),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchAddresses(query, municipalityNameOrPostalNameQuery, limit), Times.Once);
        }

        [Theory]
        [InlineData("loppem", "muni")]
        [InlineData("street", "postal")]
        public async Task WithOneWordAndMunicipalityOrPostalName1_ThenSearchStreetNames(string query, string municipalityOrPostalName)
        {
            var limit = 10;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityOrPostalName = municipalityOrPostalName}),
                    new SortingHeader("fake", SortOrder.Ascending),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchStreetNames(new[] {query}, municipalityOrPostalName, true, limit), Times.Once);
        }

        [Theory]
        [InlineData("loppem zedel", new [] {"loppem", "loppem zedel"}, "zedel")]
        [InlineData("one two three", new [] {"one", "one two", "one two three"}, "three")]
        [InlineData("one two three four", new [] {"one", "one two", "one two three", "one two three four"}, "four")]
        public async Task WithMultipleWords_ThenSearchStreetNamesWithMunicipalityOrPostalName(
            string query,
            string[] streetNames,
            string municipalityOrPostalName)
        {
            var limit = 50;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityOrPostalName = municipalityOrPostalName}),
                    new SortingHeader("fake", SortOrder.Ascending),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchStreetNames(streetNames, municipalityOrPostalName, true, limit), Times.Once);
        }
    }
}
