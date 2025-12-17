namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressBoxNumber
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
        public async Task WithInvalidBoxNumberFormat()
        {
            var act = async () => await _controller.CorrectBoxNumber(
                new CorrectAddressBoxNumberRequestValidator(FakeBoxNumberValidator.InstanceInterneBijwerker),
                MockIfMatchValidator(true),
                Fixture.Create<AddressPersistentLocalId>(),
                new CorrectAddressBoxNumberRequest { Busnummer = "12345678911"},
                ifMatchHeaderValue: null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "AdresOngeldigBusnummerformaat"
                    && e.ErrorMessage == "Ongeldig busnummerformaat."));
        }
    }
}
