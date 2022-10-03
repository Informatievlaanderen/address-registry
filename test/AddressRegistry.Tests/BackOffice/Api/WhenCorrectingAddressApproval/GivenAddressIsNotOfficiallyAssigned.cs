namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressApproval
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenAddressIsNotOfficiallyAssigned : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressIsNotOfficiallyAssigned(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                .Setup(x => x.Send(It.IsAny<AddressCorrectApprovalRequest>(), CancellationToken.None))
                .Throws(new AddressIsNotOfficiallyAssignedException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            var addressCorrectApprovalRequest = new AddressCorrectApprovalRequest
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectApproval(
                _backOfficeContext,
                new AddressCorrectApprovalRequestValidator(),
                MockIfMatchValidator(true),
                addressCorrectApprovalRequest,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e => e.ErrorCode == "AdresNietOfficeeltoegekend"
                     && e.ErrorMessage == "Deze actie is enkel toegestaan voor officieel toegekende adressen."));
        }
    }
}
