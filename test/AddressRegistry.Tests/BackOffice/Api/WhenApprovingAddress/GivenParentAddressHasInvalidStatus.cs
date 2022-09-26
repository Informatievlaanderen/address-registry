namespace AddressRegistry.Tests.BackOffice.Api.WhenApprovingAddress
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using StreetName;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenParentAddressHasInvalidStatus : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenParentAddressHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public void ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator.Setup(x => x.Send(It.IsAny<AddressApproveRequest>(), CancellationToken.None))
                .Throws(new ParentAddressHasInvalidStatusException());

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            var approveRequest = new AddressApproveRequest
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.Approve(
                _backOfficeContext,
                new AddressApproveRequestValidator(),
                MockIfMatchValidator(true),
                approveRequest,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e => e.ErrorCode == "AdresHuisnummerVoorgesteldGehistoreerdOfAfgekeurd"
                     && e.ErrorMessage == "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft."));
        }
    }
}
