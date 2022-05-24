namespace AddressRegistry.Tests.BackOffice.Validators
{
    using System.Threading;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using Infrastructure;
    using Projections.Syndication.PostalInfo;
    using Xunit;

    public class PostalCodeValidatorTests
    {
        private readonly TestSyndicationContext _syndicationContext;

        public PostalCodeValidatorTests()
        {
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
        }

        [Theory]
        [InlineData("")]
        [InlineData("test/123")]
        public async void GivenInvalidPostInfoIdUri_Invalid(string invalidPuri)
        {
            var result = await PostalCodeValidator.PostalCodeExists(_syndicationContext, invalidPuri, CancellationToken.None);
            result.Should().BeFalse();
        }

        [Fact]
        public async void GivenPostInfoIdExists_IsValid()
        {
            var postInfoId = "8200";
            var postInfoIdPuri = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}";
            _syndicationContext.AddPostalInfoLatestItem(new PostalInfoLatestItem
            {
                PostalCode = postInfoId
            });
            await _syndicationContext.SaveChangesAsync();

            var result = await PostalCodeValidator.PostalCodeExists(_syndicationContext, postInfoIdPuri, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async void GivenPostInfoIdDoesNotExists_InValid()
        {
            var postInfoId = "8200";
            var postInfoIdPuri = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}";
            var result = await PostalCodeValidator.PostalCodeExists(_syndicationContext, postInfoIdPuri, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
