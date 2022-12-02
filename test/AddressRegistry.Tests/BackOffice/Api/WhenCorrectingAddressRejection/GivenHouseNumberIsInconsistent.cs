namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressRejection
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
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

    public class GivenHouseNumberIsInconsistent : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenHouseNumberIsInconsistent(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public void ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var mockRequestValidator = new Mock<IValidator<AddressCorrectRejectionRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressCorrectRejectionRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            MockMediator.Setup(x => x.Send(It.IsAny<AddressCorrectRejectionRequest>(), CancellationToken.None))
                .Throws(new AddressBoxNumberHasInconsistentHouseNumberException());

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            var request = new AddressCorrectRejectionRequest
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectRejection(
                _backOfficeContext,
                mockRequestValidator.Object,
                MockIfMatchValidator(true),
                ResponseOptions,
                request,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e => e.ErrorCode == "AdresBusnummerHuisnummerInconsistent"
                     && e.ErrorMessage == "Deze actie is niet toegestaan op een busnummer wegens een inconsistent huisnummer."));
        }
    }
}
