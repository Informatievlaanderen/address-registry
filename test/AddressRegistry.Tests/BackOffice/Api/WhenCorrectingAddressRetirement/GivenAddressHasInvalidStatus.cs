namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressRetirement
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
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

    public class GivenAddressHasInvalidStatus : BackOfficeApiTest
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
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectRetirementRequest>(), CancellationToken.None))
                .Throws(new AddressHasInvalidStatusException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            var correctRetirementRequest = new AddressCorrectRetirementRequest
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectRetirement(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectRetirementRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                correctRetirementRequest,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e => e.ErrorCode == "AdresVoorgesteldOfAfgekeurd"
                     && e.ErrorMessage == "Deze actie is enkel toegestaan op adressen met status 'gehistoreerd'."));
        }
    }
}
