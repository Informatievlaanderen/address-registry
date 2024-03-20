namespace AddressRegistry.Api.Legacy.Tests.AddressMatch
{
    using FluentValidation.TestHelper;
    using Framework.Generate;
    using Legacy.AddressMatch.Requests;
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

            var result = _validator.TestValidate(request);

            //Act & Assert
            result.ShouldHaveValidationErrorFor(r => r.Postcode);
        }

        [Fact]
        public void BusnummerAndIndexAreMutuallyExclusive()
        {
            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";
            request.Busnummer = "C2";
            request.Index = "verd2";

            var result = _validator.TestValidate(request);

            //Act & Assert
            result.ShouldHaveValidationErrorFor(r => "Busnummer, Index");
        }
    }
}
