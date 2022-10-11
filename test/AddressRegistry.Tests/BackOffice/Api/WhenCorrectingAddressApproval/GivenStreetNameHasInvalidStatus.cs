namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressApproval
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Infrastructure;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Moq;
    using StreetName;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenStreetNameHasInvalidStatus : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenStreetNameHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectApprovalRequest>(), CancellationToken.None))
                .Throws(new StreetNameHasInvalidStatusException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            var request = new AddressCorrectApprovalRequest
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectApproval(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectApprovalRequest>(),
                MockIfMatchValidator(true),
                request,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresStraatnaamVoorgesteldOfInGebruik"
                                      && e.ErrorMessage == "Deze actie is enkel toegestaan binnen straatnamen met status 'voorgesteld' of 'inGebruik'."));
        }
    }
}
