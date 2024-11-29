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
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Elastic.AddressSearch;
    using StreetName;
    using Xunit;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public class GivenSearchQueryAndStatus
    {
        private readonly AddressSearchHandler _sut;
        private readonly Mock<IAddressApiElasticsearchClient> _mockAddressSearchApi;
        private readonly Mock<IAddressApiStreetNameElasticsearchClient> _mockAddressStreetNameSearchApi;

        public GivenSearchQueryAndStatus()
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
        [InlineData("bla bla 1", null, null)]
        [InlineData("bla bla 1", "  ", null)]
        [InlineData("bla 2A", "voorgesteld", AddressStatus.Proposed)]
        [InlineData("foo 3 bar", "inGebruik", AddressStatus.Current)]
        [InlineData("veldstraat 5 bus 1 gen", "gehistoreerd", AddressStatus.Retired)]
        [InlineData("veldstraat 5A bus 1_3 9000 get", "afgekeurd", AddressStatus.Rejected)]
        public async Task ThenExpectedAddressStatusIsPassed(
            string query,
            string? status,
            AddressStatus? expectedStatus)
        {
            var limit = 20;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, Status = status}),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x => x.SearchAddresses(query, null, expectedStatus, limit), Times.Once);
        }

        [Theory]
        [InlineData("bla bla 1", "b")]
        [InlineData("bla bla 1", " a ")]
        [InlineData("bla bla 1", ".")]
        public async Task WithIncorrectAddressStatusMapping_ThenNoResults(
            string query,
            string? status)
        {
            var result = await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, Status = status}),
                    new PaginationRequest(0, 10)),
                CancellationToken.None);

            result.Results.Should().BeEmpty();
            _mockAddressSearchApi.Verify(x => x.SearchAddresses(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<AddressStatus?>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("loppem", null, null)]
        [InlineData("loppem", "  ", null)]
        [InlineData("street", "voorgesteld", StreetNameStatus.Proposed)]
        [InlineData("street", "inGebruik", StreetNameStatus.Current)]
        [InlineData("street", "gehistoreerd", StreetNameStatus.Retired)]
        [InlineData("street", "afgekeurd", StreetNameStatus.Rejected)]
        public async Task ThenExpectedStreetNameStatusIsPassed(
            string query,
            string? status,
            StreetNameStatus? expectedStatus)
        {
            var limit = 10;
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, Status = status}),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressStreetNameSearchApi.Verify(x => x.SearchStreetNames(query, null, expectedStatus, limit), Times.Once);
        }

        [Theory]
        [InlineData("bla bla", "b")]
        [InlineData("bla bla", " a ")]
        [InlineData("bla bla", ".")]
        public async Task WithIncorrectStreetNameStatusMapping_ThenNoResults(
            string query,
            string? status)
        {
            var result = await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter { Query = query, Status = status}),
                    new PaginationRequest(0, 10)),
                CancellationToken.None);

            result.Results.Should().BeEmpty();
            _mockAddressStreetNameSearchApi.Verify(x => x.SearchStreetNames(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<StreetNameStatus?>(), It.IsAny<int>()), Times.Never);
        }
    }
}
