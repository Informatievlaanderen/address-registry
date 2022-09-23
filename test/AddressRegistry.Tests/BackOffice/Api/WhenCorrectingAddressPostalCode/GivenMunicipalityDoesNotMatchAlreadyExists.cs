namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressPostalCode
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetName;
    using FluentAssertions;
    using FluentValidation;
    using Infrastructure;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;
    using StreetName.Exceptions;

    public class GivenMunicipalityDoesNotMatchAlreadyExists : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenMunicipalityDoesNotMatchAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                .Setup(x => x.Send(It.IsAny<AddressCorrectPostalCodeRequest>(), CancellationToken.None))
                .ThrowsAsync(new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            //Act
            Func<Task> act = async () => await _controller.CorrectPostalCode(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectPostalCodeRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectPostalCodeRequest { PostInfoId = $"https://data.vlaanderen.be/id/postinfo/456" },
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(failure =>
                        failure.ErrorCode == "AdresPostinfoNietInGemeente"
                        && failure.ErrorMessage == "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente."));
        }
    }
}
