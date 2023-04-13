namespace AddressRegistry.Tests.ValueObjectTests
{
    using FluentAssertions;
    using StreetName;
    using Xunit;

    public class HouseNumberTests
    {
        [Theory]
        [InlineData("1A", "1A")]
        [InlineData("1A", "1a")]
        public void Equality(string a, string b)
        {
            new HouseNumber(a).Should().Be(new HouseNumber(b));
        }

        [Theory]
        [InlineData("1A")]
        [InlineData("1a")]
        public void ToStringShouldKeepCasing(string value)
        {
            new HouseNumber(value).ToString().Should().Be(value);
        }
    }
}
