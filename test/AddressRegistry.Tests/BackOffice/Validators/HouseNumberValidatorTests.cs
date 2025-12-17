namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using Infrastructure;
    using Xunit;

    public class HouseNumberValidatorTests
    {
        private readonly HouseNumberValidator _decentraleBijwerkerHouseNumberValidator;
        private readonly HouseNumberValidator _interneBijwekerHouseNumberValidator;

        public HouseNumberValidatorTests()
        {
            _decentraleBijwerkerHouseNumberValidator = FakeHouseNumberValidator.InstanceDecentraleBijwerker;
            _interneBijwekerHouseNumberValidator = FakeHouseNumberValidator.InstanceInterneBijwerker;
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("1A", true)]
        [InlineData("123456789A", true)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("1234567890123A", false)]
        [InlineData("123Q", false)]
        public void ValidateWhenUserIsDecentraleBijwerker(string houseNumber, bool expectedResult)
        {
            _decentraleBijwerkerHouseNumberValidator.Validate(houseNumber).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("1A", true)]
        [InlineData("123456789A", true)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("1234567890123A", false)]
        public void ValidateWhenUserIsInterneBijwerker(string houseNumber, bool expectedResult)
        {
            _interneBijwekerHouseNumberValidator.Validate(houseNumber).Should().Be(expectedResult);
        }
    }
}
