namespace AddressRegistry.Tests.BackOffice.Api.WhenRegularizingAddress
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetName;
    using StreetName.Exceptions;
    using Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressDoesNotExist : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var mockRequestValidator = new Mock<IValidator<AddressRegularizeRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressRegularizeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            MockMediator.Setup(x => x.Send(It.IsAny<AddressRegularizeRequest>(), CancellationToken.None))
                .Throws(new AddressNotFoundException(addressPersistentLocalId));

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            var regularizeRequest = new AddressRegularizeRequest()
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.Regularize(
                _backOfficeContext,
                mockRequestValidator.Object,
                Container.Resolve<IStreetNames>(),
                regularizeRequest,
                null);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaand adres.");
        }
    }
}
