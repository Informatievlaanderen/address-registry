namespace AddressRegistry.Tests.AggregateTests.ValueObjectTests
{
    using StreetName;
    using StreetName.Exceptions;
    using Xunit;

    public class HouseNumberTests
    {
        [Theory]
        [InlineData("0")]
        [InlineData("")]
        [InlineData("12345678901")]
        [InlineData("1234567890A")]
        [InlineData("1AB")]
        [InlineData("123Q")]
        public void OnInvalidHouseNumber_ThenThrowsHouseNumberHasInvalidFormatException(string houseNumber)
        {
            Assert.Throws<HouseNumberHasInvalidFormatException>(() => HouseNumber.Create(houseNumber));
        }
    }
}
