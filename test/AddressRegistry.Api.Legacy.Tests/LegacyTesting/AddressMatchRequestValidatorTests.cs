namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting
{
    using AddressMatch.Requests;
    using FluentValidation.TestHelper;
    using Generate;
    using Xunit;

    public class AddressMatchRequestValidatorTests
    {
        private readonly AddressMatchRequestValidator _validator;

        public AddressMatchRequestValidatorTests()
        {
            _validator = new AddressMatchRequestValidator();
        }

        [Fact]
        public void PostcodeShouldBeNumeric()
        {
            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Postcode = "NAN";

            //Act & Assert
            _validator.ShouldHaveValidationErrorFor(r => r.Postcode, request);
        }

        [Fact]
        public void BusnummerAndIndexAreMutuallyExclusive()
        {
            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";
            request.Busnummer = "C2";
            request.Index = "verd2";

            //Act & Assert
            _validator.ShouldHaveValidationErrorFor(r => "Busnummer, Index", request);
        }
    }
}
