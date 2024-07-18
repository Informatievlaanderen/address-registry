namespace AddressRegistry.Tests.BackOffice.Api.WhenChangingAddressPostalCode
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using StreetName;
    using FluentAssertions;
    using FluentValidation;
    using Infrastructure;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;

    public class GivenInvalidRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public void WithNonExistentPostInfo_ThenThrowsValidationException()
        {
            var nonExistentPostInfo = PostInfoPuri + "123456";
            var postalConsumerContext = new FakePostalConsumerContextFactory().CreateDbContext();

            Func<Task> act = async () => await _controller.ChangePostalCode(new ChangeAddressPostalCodeRequestValidator(postalConsumerContext),
                MockIfMatchValidator(true),
                Fixture.Create<AddressPersistentLocalId>(),
                new ChangeAddressPostalCodeRequest()
                {
                    PostInfoId = nonExistentPostInfo
                },
                null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "AdresPostinfoNietGekendValidatie"
                    && e.ErrorMessage == $"De postinfo '{nonExistentPostInfo}' is niet gekend in het postinforegister."));
        }
    }
}
