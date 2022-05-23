namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using Xunit;

    public class BoxNumberValidatorTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("0", false)]
        [InlineData("1234567890123A", false)]
        [InlineData("bus 1", false)]
        [InlineData("BUS1A", false)]
        [InlineData("01", true)]
        [InlineData("0A", true)]
        [InlineData("abc", true)]
        [InlineData("123456789A", true)]
        public void Validate(string boxNumber, bool expectedResult)
        {
            BoxNumberValidator.IsValid(boxNumber).Should().Be(expectedResult);
        }
    }
}
