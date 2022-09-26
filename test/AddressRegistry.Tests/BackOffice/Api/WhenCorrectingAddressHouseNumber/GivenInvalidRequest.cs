namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressHouseNumber
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using StreetName;
    using Infrastructure;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Moq;
    using StreetName.Exceptions;
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
        public async Task WithInvalidHouseNumber_ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectHouseNumberRequest>(), CancellationToken.None))
                .Throws(new HouseNumberHasInvalidFormatException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            Func<Task> act = async () => await _controller.CorrectHouseNumber(
                _backOfficeContext,
                new AddressCorrectHouseNumberRequestValidator(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101" },
                null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "AdresOngeldigHuisnummerformaat"
                        && e.ErrorMessage == "Ongeldig huisnummerformaat."));
        }
    }
}
