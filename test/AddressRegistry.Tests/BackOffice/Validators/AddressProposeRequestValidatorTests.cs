namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Address.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentValidation.TestHelper;
    using Infrastructure;
    using Projections.Syndication.PostalInfo;
    using Xunit;

    public class AddressProposeRequestValidatorTests
    {
        private readonly TestSyndicationContext _syndicationContext;
        private readonly AddressProposeRequestValidator _validator;

        public AddressProposeRequestValidatorTests()
        {
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            _validator = new AddressProposeRequestValidator(_syndicationContext);
        }

        [Fact]
        public void GivenEmptyPostInfoId_ValidationError()
        {
            var result = _validator.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "",
                Huisnummer = "11"
            });

            result
                .ShouldHaveValidationErrorFor($"{nameof(AddressProposeRequest.PostInfoId)}")
                .WithErrorMessage("Ongeldige postinfoId.");
        }

        [Fact]
        public void GivenInvalidPostInfoIdUri_ValidationError()
        {
            var invalidUri = "11111";

            _syndicationContext.AddPostalInfoLatestItem(new PostalInfoLatestItem
            {
                PostalCode = "8200",
            });

            var result = _validator.TestValidate(new AddressProposeRequest
            {
                PostInfoId = invalidUri,
                Huisnummer = "11"
            });

            result
                .ShouldHaveValidationErrorFor($"{nameof(AddressProposeRequest.PostInfoId)}")
                .WithErrorMessage("Ongeldige postinfoId.");
        }

        [Fact]
        public void GivenNonExistentPostInfoId_ValidationError()
        {
            var nonExistentPostInfoId = "12345";
            _syndicationContext.AddPostalInfoLatestItem(new PostalInfoLatestItem
            {
                PostalCode = "8200"
            });

            var result = _validator.TestValidate(new AddressProposeRequest
            {
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{nonExistentPostInfoId}",
                Huisnummer = "11"
            });

            result
                .ShouldHaveValidationErrorFor($"{nameof(AddressProposeRequest.PostInfoId)}")
                .WithErrorMessage("Ongeldige postinfoId.");
        }

        [Fact]
        public void GivenPostInfoIdExists_IsValid()
        {
            var postInfoId = "8200";
            _syndicationContext.AddPostalInfoLatestItem(new PostalInfoLatestItem
            {
                PostalCode = postInfoId
            });

            var result = _validator.TestValidate(new AddressProposeRequest
            {
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = "11"
            });

            result
                .ShouldNotHaveAnyValidationErrors();
        }
    }
}
