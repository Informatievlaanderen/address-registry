namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using FluentValidation;
    using FluentValidation.Results;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenStreetNameExists : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTheAddressIsProposed()
        {
            var streetNamePersistentId = new StreetNamePersistentLocalId(123);
            var postInfoId = new PersistentLocalId(456);

            var mockRequestValidator = new Mock<IValidator<AddressProposeRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressProposeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            MockMediator.Setup(x => x.Send(It.IsAny<AddressProposeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new PersistentLocalIdETagResponse(1, string.Empty)) );

            var request = new AddressProposeRequest
            {
                StraatNaamId = $"https://data.vlaanderen.be/id/straatnaam/{streetNamePersistentId}",
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = "11",
            };

            //Act
            await _controller.Propose(
                ResponseOptions,
                mockRequestValidator.Object,
                request);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<AddressProposeRequest>(), CancellationToken.None));
        }
    }
}
