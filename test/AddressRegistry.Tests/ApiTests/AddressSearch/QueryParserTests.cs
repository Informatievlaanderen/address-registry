namespace AddressRegistry.Tests.ApiTests.AddressSearch
{
    using Api.Oslo.Address.Search;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class QueryParserTests
    {
        [Theory]
        [InlineData("kerk", false, "kerk", null)]
        [InlineData("kerk 123", false, "kerk 123", null)]
        [InlineData("kerk 1234", true, "kerk ", "10000")]
        [InlineData("kerk 1234,", true, "kerk ,", "10000")]
        [InlineData("kerk 12345", false, "kerk 12345", null)]
        [InlineData("kerk 1234 1234", false, "kerk 1234 1234", null)]
        public void PostalCodeIsRecognized(string query, bool expectedResult, string expectedQuery, string? expectedNisCode)
        {
            var postalCacheMock = new Mock<IPostalCache>();

            postalCacheMock
                .Setup(x => x.GetNisCodeByPostalCode("1234"))
                .Returns("10000");

            var queryParser = new QueryParser(postalCacheMock.Object);
            var result = queryParser.TryExtractNisCodeViaPostalCode(ref query, out var nisCode);
            result.Should().Be(expectedResult);
            query.Should().Be(expectedQuery);
            nisCode.Should().Be(expectedNisCode);
        }
    }
}
