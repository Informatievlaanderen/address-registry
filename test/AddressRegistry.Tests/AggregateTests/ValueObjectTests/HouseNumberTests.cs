namespace AddressRegistry.Tests.AggregateTests.ValueObjectTests
{
    using StreetName;
    using StreetName.Exceptions;
    using Xunit;

    public class HouseNumberTests
    {
        [Theory]
        //[InlineData("0")] is valid since GAWR-7234
        [InlineData("")]
        [InlineData("12345678901")]
        [InlineData("1234567890A")]
        //[InlineData("1AB")] is valid since GAWR-7234
        //[InlineData("1.AB")] is valid since GAWR-7234
        //[InlineData("1/AB")] is valid since GAWR-7234
        //[InlineData("A")] is valid since GAWR-7234
        public void OnInvalidHouseNumber_ThenThrowsHouseNumberHasInvalidFormatException(string houseNumber)
        {
            Assert.Throws<HouseNumberHasInvalidFormatException>(() => HouseNumber.Create(houseNumber));
        }
    }
}
