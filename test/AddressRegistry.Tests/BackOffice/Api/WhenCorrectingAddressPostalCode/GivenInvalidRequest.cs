namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressPostalCode
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using StreetName;
    using Infrastructure;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInvalidRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public void WithNonExistentPostInfo_ThenThrowsValidationException()
        {
            var nonExistentPostInfo = PostInfoPuri + "123456";
            var syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();

            Func<Task> act = async () => await _controller.CorrectPostalCode(
                _backOfficeContext,
                new CorrectAddressPostalCodeRequestValidator(syndicationContext),
                MockIfMatchValidator(true),
                Fixture.Create<AddressPersistentLocalId>(),
                new CorrectAddressPostalCodeRequest { PostInfoId = nonExistentPostInfo },
                null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "AdresPostinfoNietGekendValidatie"
                        && e.ErrorMessage == $"De postinfo '{nonExistentPostInfo}' is niet gekend in het postinforegister."));
        }
    }
}
