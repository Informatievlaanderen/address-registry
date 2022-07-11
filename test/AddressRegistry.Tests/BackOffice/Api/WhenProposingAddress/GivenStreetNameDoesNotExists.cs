namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenStreetNameDoesNotExists : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;

        public GivenStreetNameDoesNotExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            var nonExistentStreetNameId = new StreetNamePersistentLocalId(123);
            var postInfoId = new PersistentLocalId(456);

            var mockRequestValidator = new Mock<IValidator<AddressProposeRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressProposeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            MockMediator.Setup(x => x.Send(It.IsAny<AddressProposeRequest>(), CancellationToken.None))
                .Throws(new AggregateNotFoundException(nonExistentStreetNameId.ToString(), typeof(StreetName)));

            var streetNamePuri = $"https://data.vlaanderen.be/id/straatnaam/{nonExistentStreetNameId}";
            var body = new AddressProposeRequest
            {
                StraatNaamId = streetNamePuri,
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/{postInfoId}",
                Huisnummer = "11"
            };

            //Act
            Func<Task> act = async () => await _controller.Propose(
                ResponseOptions,
                mockRequestValidator.Object,
                body);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(
                        failure => failure.ErrorCode == "AdresStraatnaamNietGekendValidatie"
                                   && failure.ErrorMessage == $"De straatnaam '{streetNamePuri}' is niet gekend in het straatnaamregister."
                                   && failure.PropertyName == nameof(body.StraatNaamId)));
        }
    }
}
