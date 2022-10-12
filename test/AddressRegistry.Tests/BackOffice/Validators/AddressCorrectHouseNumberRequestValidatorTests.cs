namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class AddressCorrectHouseNumberRequestValidatorTests
    {
        private readonly AddressCorrectHouseNumberRequestValidator _sut;

        public AddressCorrectHouseNumberRequestValidatorTests()
        {
            _sut = new AddressCorrectHouseNumberRequestValidator();
        }

        [Fact]
        public void WhenHouseNumberIsEmpty_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressCorrectHouseNumberRequest()
            {
                Huisnummer = string.Empty
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressCorrectHouseNumberRequest.Huisnummer))
                .WithErrorCode("AdresOngeldigHuisnummerformaat")
                .WithErrorMessage("Ongeldig huisnummerformaat.");
        }

        [Fact]
        public void WhenHouseNumberFormatIsInvalid_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressCorrectHouseNumberRequest()
            {
                Huisnummer = "|€{[^"
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressCorrectHouseNumberRequest.Huisnummer))
                .WithErrorCode("AdresOngeldigHuisnummerformaat")
                .WithErrorMessage("Ongeldig huisnummerformaat.");
        }
    }
}
