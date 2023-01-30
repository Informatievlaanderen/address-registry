namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class CorrectAddressBoxNumberRequestValidatorTests
    {
        private readonly CorrectAddressBoxNumberRequestValidator _sut;

        public CorrectAddressBoxNumberRequestValidatorTests()
        {
            _sut = new CorrectAddressBoxNumberRequestValidator();
        }

        [Fact]
        public void WhenBoxNumberIsEmpty_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressBoxNumberRequest
            {
                Busnummer = string.Empty
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressBoxNumberRequest.Busnummer))
                .WithErrorCode("AdresOngeldigBusnummerformaat")
                .WithErrorMessage("Ongeldig busnummerformaat.");
        }

        [Fact]
        public void WhenBoxNumberFormatIsInvalid_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressBoxNumberRequest
            {
                Busnummer = "|€{[^"
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressBoxNumberRequest.Busnummer))
                .WithErrorCode("AdresOngeldigBusnummerformaat")
                .WithErrorMessage("Ongeldig busnummerformaat.");
        }
    }
}
