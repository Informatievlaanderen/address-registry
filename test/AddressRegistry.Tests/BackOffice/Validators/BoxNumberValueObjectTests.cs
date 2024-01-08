namespace AddressRegistry.Tests.BackOffice.Validators
{
    using FluentAssertions;
    using StreetName;
    using Xunit;

    public class BoxNumberValueObjectTests
    {
        [Theory]
        [InlineData("01")]
        [InlineData("0A")]
        [InlineData("abc")]
        [InlineData("123456789A")]
        [InlineData("1/A")]
        [InlineData("1.A")]
        [InlineData("46/0.1")]
        [InlineData("46.0/1")]
        public void HasValidFormat(string boxNumber)
        {
            BoxNumber.HasValidFormat(boxNumber).Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("1234567890123A")]
        [InlineData("bus 1")]
        [InlineData("BUS1A")]
        [InlineData("1.")]
        [InlineData("1/")]
        [InlineData("/A")]
        [InlineData(".A")]
        [InlineData("A..1")]
        [InlineData("A//1")]
        [InlineData("A./1")]
        [InlineData("A/.1")]
        public void HasInvalidFormat(string boxNumber)
        {
            BoxNumber.HasValidFormat(boxNumber).Should().BeFalse();
        }
    }
}
