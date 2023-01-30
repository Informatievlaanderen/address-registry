namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class CorrectAddressHouseNumberRequestValidatorTests
    {
        private readonly CorrectAddressHouseNumberRequestValidator _sut;

        public CorrectAddressHouseNumberRequestValidatorTests()
        {
            _sut = new CorrectAddressHouseNumberRequestValidator();
        }

        [Fact]
        public void WhenHouseNumberIsEmpty_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressHouseNumberRequest
            {
                Huisnummer = string.Empty
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressHouseNumberRequest.Huisnummer))
                .WithErrorCode("AdresOngeldigHuisnummerformaat")
                .WithErrorMessage("Ongeldig huisnummerformaat.");
        }

        [Fact]
        public void WhenHouseNumberFormatIsInvalid_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressHouseNumberRequest
            {
                Huisnummer = "|€{[^"
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressHouseNumberRequest.Huisnummer))
                .WithErrorCode("AdresOngeldigHuisnummerformaat")
                .WithErrorMessage("Ongeldig huisnummerformaat.");
        }
    }
}
