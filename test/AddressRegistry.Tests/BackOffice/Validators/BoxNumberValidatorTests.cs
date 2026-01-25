namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using Infrastructure;
    using Xunit;

    public class BoxNumberValidatorTests
    {
        private readonly BoxNumberValidator _decentraleBijwerkerBoxNumberValidator;
        private readonly BoxNumberValidator _interneBijwerkerBoxNumberValidator;

        public BoxNumberValidatorTests()
        {
            _decentraleBijwerkerBoxNumberValidator = FakeBoxNumberValidator.InstanceDecentraleBijwerker;
            _interneBijwerkerBoxNumberValidator = FakeBoxNumberValidator.InstanceInterneBijwerker;
        }

        [Theory]
        [InlineData("01", true)]
        [InlineData("0A", true)]
        [InlineData("abc", true)]
        [InlineData("-abc", false)]
        [InlineData("123456789A", true)]
        [InlineData("1/A", true)]
        [InlineData("1.A", true)]
        [InlineData("46/0.1", true)]
        [InlineData("46.0/1", true)]
        [InlineData("-46.0/1", false)]
        [InlineData("", false)]
        [InlineData("-", false)]
        [InlineData("0", false)]
        [InlineData("1234567890123A", false)]
        [InlineData("-1234567890", false)]
        [InlineData("bus 1", false)]
        [InlineData("BUS1A", false)]
        [InlineData("1.", false)]
        [InlineData("1/", false)]
        [InlineData("/A", false)]
        [InlineData(".A", false)]
        [InlineData("A..1", false)]
        [InlineData("A//1", false)]
        [InlineData("A./1", false)]
        [InlineData("A/.1", false)]
        public void ValidateWhenUserIsDecentraleBijwerker(string boxNumber, bool expectedResult)
        {
            _decentraleBijwerkerBoxNumberValidator.Validate(boxNumber).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("01", true)]
        [InlineData("0A", true)]
        [InlineData("abc", true)]
        [InlineData("-abc", true)]
        [InlineData("123456789A", true)]
        [InlineData("1/A", true)]
        [InlineData("1.A", true)]
        [InlineData("46/0.1", true)]
        [InlineData("46.0/1", true)]
        [InlineData("-46.0/1", true)]
        [InlineData("", false)]
        [InlineData("-", false)]
        [InlineData("0", false)]
        [InlineData("1234567890123A", false)]
        [InlineData("-1234567890", false)]
        [InlineData("bus 1", false)]
        [InlineData("BUS1A", false)]
        [InlineData("1.", false)]
        [InlineData("1/", false)]
        [InlineData("/A", false)]
        [InlineData(".A", false)]
        //[InlineData("A..1", false)] valid since GAWR-7233
        //[InlineData("A//1", false)] valid since GAWR-7233
        //[InlineData("A./1", false)] valid since GAWR-7233
        //[InlineData("A/.1", false)] valid since GAWR-7233
        public void ValidateWhenUserIsInterneBijwerker(string boxNumber, bool expectedResult)
        {
            _interneBijwerkerBoxNumberValidator.Validate(boxNumber).Should().Be(expectedResult);
        }
    }
}
