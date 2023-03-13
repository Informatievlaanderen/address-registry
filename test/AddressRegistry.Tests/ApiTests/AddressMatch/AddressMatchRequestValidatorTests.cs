namespace AddressRegistry.Tests.ApiTests.AddressMatch
{
    using Api.Oslo.AddressMatch.Requests;
    using FluentValidation.TestHelper;
    using Xunit;

    public class AddressMatchRequestValidatorTests
    {
        private readonly AddressMatchRequestValidator _validator;

        public AddressMatchRequestValidatorTests()
            => _validator = new AddressMatchRequestValidator();

        [Fact]
        public void PostcodeShouldBeNumeric()
        {
            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Postcode = "NAN";

            //Act & Assert
            _validator.ShouldHaveValidationErrorFor(r => r.Postcode, request);
        }
    }
}
