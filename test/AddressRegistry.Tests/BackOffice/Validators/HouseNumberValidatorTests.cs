namespace AddressRegistry.Tests.BackOffice.Validators
{
    using FluentAssertions;
    using StreetName;
    using Xunit;

    public class HouseNumberValidatorTests
    {
        [Theory]
        [InlineData("1", true)]
        [InlineData("1A", true)]
        [InlineData("123456789A", true)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("1234567890123A", false)]
        public void Validate(string houseNumber, bool expectedResult)
        {
            HouseNumber.HasValidFormat(houseNumber).Should().Be(expectedResult);
        }
    }
}
