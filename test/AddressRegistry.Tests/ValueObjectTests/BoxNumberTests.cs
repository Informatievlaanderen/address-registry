namespace AddressRegistry.Tests.ValueObjectTests
{
    using FluentAssertions;
    using StreetName;
    using Xunit;

    public class BoxNumberTests
    {
        [Theory]
        [InlineData("A", "A")]
        [InlineData("A", "a")]
        public void Equality(string a, string b)
        {
            new BoxNumber(a).Should().Be(new BoxNumber(b));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("a")]
        public void ToStringShouldKeepCasing(string value)
        {
            new BoxNumber(value).ToString().Should().Be(value);
        }
    }
}
