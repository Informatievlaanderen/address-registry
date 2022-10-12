namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class AddressCorrectBoxNumberRequestValidatorTests
    {
        private readonly AddressCorrectBoxNumberRequestValidator _sut;

        public AddressCorrectBoxNumberRequestValidatorTests()
        {
            _sut = new AddressCorrectBoxNumberRequestValidator();
        }

        [Fact]
        public void WhenBoxNumberIsEmpty_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressCorrectBoxNumberRequest()
            {
                Busnummer = string.Empty
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressCorrectBoxNumberRequest.Busnummer))
                .WithErrorCode("AdresOngeldigBusnummerformaat")
                .WithErrorMessage("Ongeldig busnummerformaat.");
        }

        [Fact]
        public void WhenBoxNumberFormatIsInvalid_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressCorrectBoxNumberRequest()
            {
                Busnummer = "|€{[^"
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressCorrectBoxNumberRequest.Busnummer))
                .WithErrorCode("AdresOngeldigBusnummerformaat")
                .WithErrorMessage("Ongeldig busnummerformaat.");
        }
    }
}
