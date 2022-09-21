namespace AddressRegistry.Tests.BackOffice.Api.WhenRetiringAddress
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using StreetName;
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenAddressWasProposed : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressWasProposed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenNoContentWithETagResultIsReturned()
        {
            var lastEventHash = "eventhash";
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var mockRequestValidator = new Mock<IValidator<AddressRetireRequest>>();
            mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<AddressRetireRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressRetireRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ETagResponse(string.Empty, lastEventHash)));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            var result = (AcceptedWithETagResult)await _controller.Retire(
                _backOfficeContext,
                mockRequestValidator.Object,
                MockIfMatchValidator(true),
                ResponseOptions,
                request: new AddressRetireRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                ifMatchHeaderValue: null);

            //Assert
            result.ETag.Should().Be(lastEventHash);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var mockRequestValidator = new Mock<IValidator<AddressRetireRequest>>();
            mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<AddressRetireRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            await _backOfficeContext.SaveChangesAsync();

            //Act
            var result = await _controller.Retire(
                _backOfficeContext,
                mockRequestValidator.Object,
                MockIfMatchValidator(false),
                ResponseOptions,
                request: new AddressRetireRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
