namespace AddressRegistry.Tests.BackOffice.Validators
{
    using System.Threading;
    using AddressRegistry.Api.BackOffice.Validators;
    using Consumer.Read.Postal.Projections;
    using FluentAssertions;
    using Infrastructure;
    using Xunit;

    public class PostalCodeValidatorTests
    {
        private readonly FakePostalConsumerContext _postalConsumerContext;

        public PostalCodeValidatorTests()
        {
            _postalConsumerContext = new FakePostalConsumerContextFactory().CreateDbContext();
        }

        [Theory]
        [InlineData("")]
        [InlineData("test/123")]
        public async void GivenInvalidPostInfoIdUri_Invalid(string invalidPuri)
        {
            var result = await PostalCodeValidator.PostalCodeExists(_postalConsumerContext, invalidPuri, CancellationToken.None);
            result.Should().BeFalse();
        }

        [Fact]
        public async void GivenPostInfoIdExists_IsValid()
        {
            var postInfoId = "8200";
            var postInfoIdPuri = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}";
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = postInfoId
            });
            await _postalConsumerContext.SaveChangesAsync();

            var result = await PostalCodeValidator.PostalCodeExists(_postalConsumerContext, postInfoIdPuri, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async void GivenPostInfoIdDoesNotExists_InValid()
        {
            var postInfoId = "8200";
            var postInfoIdPuri = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}";
            var result = await PostalCodeValidator.PostalCodeExists(_postalConsumerContext, postInfoIdPuri, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
