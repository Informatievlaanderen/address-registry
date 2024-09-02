namespace AddressRegistry.Tests.ApiTests.AddressSearch
{
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
    using Xunit;

    public class GivenSearchQueryAndMunicipalityOrPostalName
    {
        private readonly AddressSearchHandler _sut;
        private readonly Mock<IAddressApiElasticsearchClient> _mockAddressSearchApi;

        public GivenSearchQueryAndMunicipalityOrPostalName()
        {
            _mockAddressSearchApi = new Mock<IAddressApiElasticsearchClient>();
            var mockResponseOptions = new Mock<IOptions<ResponseOptions>>();

            _sut = new AddressSearchHandler(_mockAddressSearchApi.Object, mockResponseOptions.Object);
        }

        [Theory]
        [InlineData("bla bla 1")]
        [InlineData("bla 2A")]
        [InlineData("foo 3 bar")]
        public async Task WithHouseNumber_ThenNoResults(string query)
        {
            var result = await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query }),
                    new SortingHeader("fake", SortOrder.Ascending),
                    new NoPaginationRequest()),
                CancellationToken.None);

            result.Results.Should().BeEmpty();
        }

        [Theory]
        [InlineData("loppem", "muni")]
        [InlineData("street", "postal")]
        public async Task WithOneWordAndMunicipalityOrPostalName1_ThenSearchStreetNames(string query, string municipalityOrPostalName)
        {
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityOrPostalName = municipalityOrPostalName}),
                    new SortingHeader("fake", SortOrder.Ascending),
                    new NoPaginationRequest()),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchStreetNames(new[] {query}, municipalityOrPostalName, It.IsAny<int>()), Times.Once);
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
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, MunicipalityOrPostalName = municipalityOrPostalName}),
                    new SortingHeader("fake", SortOrder.Ascending),
                    new NoPaginationRequest()),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchStreetNames(streetNames, municipalityOrPostalName, It.IsAny<int>()), Times.Once);
        }
    }
}
