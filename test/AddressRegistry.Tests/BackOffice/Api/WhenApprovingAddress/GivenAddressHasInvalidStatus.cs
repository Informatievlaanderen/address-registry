namespace AddressRegistry.Tests.BackOffice.Api.WhenApprovingAddress
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using StreetName;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;
    using AddressGeometry = Address.AddressGeometry;
    using AddressId = Address.AddressId;
    using AddressStatus = Address.AddressStatus;
    using PostalCode = Address.PostalCode;
    using StreetNameId = Address.StreetNameId;

    public class GivenAddressHasInvalidStatus : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenAddressHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        //TODO: add rejected
        [Fact]
        public void ThenThrowApiException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var mockRequestValidator = new Mock<IValidator<AddressApproveRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressApproveRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            MockMediator.Setup(x => x.Send(It.IsAny<AddressApproveRequest>(), CancellationToken.None))
                .Throws(new AddressCannotBeApprovedException(AddressRegistry.StreetName.AddressStatus.Rejected));

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
                mockRequestValidator.Object,
                Container.Resolve<IStreetNames>(),
                approveRequest,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e => e.ErrorCode == "AdresGehistoreerdOfAfgekeurd"
                     && e.ErrorMessage == "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'."));
        }
    }
}
