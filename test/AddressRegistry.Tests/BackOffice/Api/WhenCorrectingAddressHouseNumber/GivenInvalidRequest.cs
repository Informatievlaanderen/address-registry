namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressHouseNumber
{
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Infrastructure;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInvalidRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task WithInvalidHouseNumberFormat()
        {
            var act = async () => await _controller.CorrectHouseNumber(
                new CorrectAddressHouseNumberRequestValidator(FakeHouseNumberValidator.InstanceInterneBijwerker),
                MockIfMatchValidator(true),
                Fixture.Create<AddressPersistentLocalId>(),
                new CorrectAddressHouseNumberRequest { Huisnummer = "INVALID."},
                ifMatchHeaderValue: null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "AdresOngeldigHuisnummerformaat"
                    && e.ErrorMessage == "Ongeldig huisnummerformaat."));
        }
    }
}
