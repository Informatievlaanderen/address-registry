namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressHouseNumber
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetName;
    using StreetName.Exceptions;
    using Infrastructure;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenAddressHasInvalidStatus : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectHouseNumberRequest>(), CancellationToken.None))
                .Throws(new AddressHasInvalidStatusException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            //Act
            Func<Task> act = async () => await _controller.CorrectHouseNumber(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectHouseNumberRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101"},
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e =>
                         e.ErrorCode == "AdresGehistoreerdOfAfgekeurd"
                         && e.ErrorMessage == "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'."));
        }
    }
}
