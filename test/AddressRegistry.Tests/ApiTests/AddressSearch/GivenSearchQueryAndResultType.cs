namespace AddressRegistry.Tests.ApiTests.AddressSearch
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Oslo.Address.Search;
    using Api.Oslo.Infrastructure.Elastic;
    using Api.Oslo.Infrastructure.Options;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Consumer.Read.StreetName.Projections.Elastic;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure.Elastic;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Elastic.AddressSearch;
    using StreetName;
    using Xunit;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public class GivenSearchQueryAndResultType
    {
        private readonly AddressSearchHandler _sut;
        private readonly Mock<IAddressApiElasticsearchClient> _mockAddressSearchApi;
        private readonly Mock<IAddressApiStreetNameElasticsearchClient> _mockAddressStreetNameSearchApi;
        private readonly Fixture _fixture;
        private readonly Mock<IPostalCache> _mockPostalCache;

        public GivenSearchQueryAndResultType()
        {
            _fixture = new Fixture();
            _fixture.Customizations.Add(new WithUniqueInteger());

            _mockAddressSearchApi = new Mock<IAddressApiElasticsearchClient>();
            _mockAddressStreetNameSearchApi = new Mock<IAddressApiStreetNameElasticsearchClient>();

            _mockAddressSearchApi.Setup(x => x.SearchAddresses(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AddressStatus?>(), It.IsAny<int>()))
                .ReturnsAsync(new AddressSearchResult(new List<AddressSearchDocument>(), 0));
            _mockAddressStreetNameSearchApi.Setup(x =>
                    x.SearchStreetNames(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StreetNameStatus?>(), It.IsAny<int>()))
                .ReturnsAsync(new StreetNameSearchResult(new List<StreetNameSearchDocument>(), 0));

            var options = _fixture.Create<ResponseOptions>();
            options.StraatnaamDetailUrl = "https://www.straatnaam.be";
            options.DetailUrl = "https://www.adres.be";
            var responseOptions = new OptionsWrapper<ResponseOptions>(options);

            _mockPostalCache = new Mock<IPostalCache>();
            _sut = new AddressSearchHandler(
                _mockAddressSearchApi.Object,
                _mockAddressStreetNameSearchApi.Object,
                responseOptions,
                Mock.Of<IMunicipalityCache>(),
                new QueryParser(_mockPostalCache.Object));
        }

        [Fact]
        public async Task WithResultTypeIsAddress_ThenSearchAddressesOnly()
        {
            var limit = 10;
            var query = _fixture.Create<string>();
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter
                    {
                        Query = query,
                        ResultType = ResultType.Address
                    }),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x =>
                x.SearchAddresses(query, null, null, limit), Times.Once);

            _mockAddressStreetNameSearchApi.Verify(x =>
                x.SearchStreetNames(It.IsAny<string>(), null, null, limit), Times.Never);
        }

        [Fact]
        public async Task WithResultTypeIsStreetName_ThenSearchStreetNamesOnly()
        {
            var limit = 10;
            var query = _fixture.Create<string>();
            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter
                    {
                        Query = query,
                        ResultType = ResultType.StreetName
                    }),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x =>
                x.SearchAddresses(It.IsAny<string>(), null, null, limit), Times.Never);

            _mockAddressStreetNameSearchApi.Verify(x =>
                x.SearchStreetNames(query, null, null, limit), Times.Once);
        }

        [Fact]
        public async Task WithResultTypeIsStreetNameAndQueryContainsExistingPostalCode_ThenSearchStreetNamesOnlyWithinMunicipality()
        {
            var limit = 10;
            var queryWithoutPostalCode = "bla bla bli ";
            var query = $"{queryWithoutPostalCode}9030";

            _mockPostalCache
                .Setup(x => x.GetNisCodeByPostalCode("9030"))
                .Returns("44012");

            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter
                    {
                        Query = query,
                        ResultType = ResultType.StreetName
                    }),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x =>
                x.SearchAddresses(It.IsAny<string>(), null, null, limit), Times.Never);

            _mockAddressStreetNameSearchApi.Verify(x =>
                x.SearchStreetNames(queryWithoutPostalCode, "44012", null, limit), Times.Once);
        }

        [Fact]
        public async Task WithResultTypeIsStreetNameAndQueryContainsNonExistingPostalCode_ThenSearchStreetNamesOnlyWithinMunicipality()
        {
            var limit = 10;
            var queryWithoutPostalCode = _fixture.Create<string>() + " ";
            var query = $"{queryWithoutPostalCode}9030";

            await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter
                    {
                        Query = query,
                        ResultType = ResultType.StreetName
                    }),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x =>
                x.SearchAddresses(It.IsAny<string>(), null, null, limit), Times.Never);

            _mockAddressStreetNameSearchApi.Verify(x =>
                x.SearchStreetNames(query, null, null, limit), Times.Once);
        }

        [Fact]
        public async Task GivenMatchingStreetNamesEqualsLimit_WithNoResultType_ThenOnlyReturnStreetNames()
        {
            var limit = 10;
            var query = _fixture.Create<string>();

            var addresses = _fixture.CreateMany<AddressSearchDocument>(limit).ToList();
            _mockAddressSearchApi
                .Setup(x => x.SearchAddresses(query, null, null, limit))
                .ReturnsAsync(
                    new AddressSearchResult(
                        addresses,
                        limit,
                        Language.nl)
                    );

            var streetNames = _fixture.CreateMany<StreetNameSearchDocument>(limit).ToList();
            _mockAddressStreetNameSearchApi
                .Setup(x => x.SearchStreetNames(query, null, null, limit))
                .ReturnsAsync(
                    new StreetNameSearchResult(
                        streetNames,
                        limit,
                        Language.nl)
                    );

           var result = await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter
                    {
                        Query = query
                    }),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

            _mockAddressSearchApi.Verify(x =>
                x.SearchAddresses(It.IsAny<string>(), null, null, limit), Times.Never);

           var resultObjectIds = result.Results.Select(x => int.Parse(x.ObjectId)).ToList();
           resultObjectIds
               .Should().BeEquivalentTo(streetNames.Select(x => x.StreetNamePersistentLocalId));
        }

        [Theory]
        [InlineData(10, 4)]
        [InlineData(10, 0)]
        public async Task GivenMatchingStreetNamesLessThanLimit_WithNoResultType_ThenReturnStreetNamesFollowedByAddresses(
            int limit, int numberOfStreetNames)
        {
            var query = _fixture.Create<string>();

            var addresses = _fixture.CreateMany<AddressSearchDocument>(limit).ToList();
            _mockAddressSearchApi
                .Setup(x => x.SearchAddresses(query, null, null, limit))
                .ReturnsAsync(
                    new AddressSearchResult(
                        addresses,
                        limit,
                        Language.nl)
                    );

            var streetNames = _fixture.CreateMany<StreetNameSearchDocument>(numberOfStreetNames).ToList();
            _mockAddressStreetNameSearchApi
                .Setup(x => x.SearchStreetNames(query, null, null, limit))
                .ReturnsAsync(
                    new StreetNameSearchResult(
                        streetNames,
                        limit,
                        Language.nl)
                    );

           var result = await _sut.Handle(
                new AddressSearchRequest(
                    new FilteringHeader<AddressSearchFilter>(new AddressSearchFilter
                    {
                        Query = query
                    }),
                    new PaginationRequest(0, limit)),
                CancellationToken.None);

           var resultObjectIds = result.Results.Select(x => int.Parse(x.ObjectId)).ToList();

           resultObjectIds.Should().HaveCount(limit);

           var expectedObjectIds = streetNames.Select(x => x.StreetNamePersistentLocalId)
               .Concat(addresses.Select(x => x.AddressPersistentLocalId))
               .Take(limit)
               .ToList();

           resultObjectIds.Should().BeEquivalentTo(expectedObjectIds, options => options.WithStrictOrdering());
        }
    }
}
