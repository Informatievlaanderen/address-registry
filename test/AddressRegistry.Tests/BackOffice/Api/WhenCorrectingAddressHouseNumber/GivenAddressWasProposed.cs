namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressHouseNumber
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Validators;
    using StreetName;
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Moq;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

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
        public async Task ThenAcceptedWithETagResultIsReturned()
        {
            var lastEventHash = "eventhash";
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectHouseNumberRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ETagResponse(string.Empty, lastEventHash)));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = (AcceptedWithETagResult) await _controller.CorrectHouseNumber(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectHouseNumberRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101" },
                null, CancellationToken.None);

            //Assert
            result.ETag.Should().Be(lastEventHash);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

           await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = await _controller.CorrectHouseNumber(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectHouseNumberRequest>(),
                MockIfMatchValidator(false),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101" },
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task WithDuplicateHouseNumber_ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectHouseNumberRequest>(), CancellationToken.None))
                .Throws(new ParentAddressAlreadyExistsException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            Func<Task> act = async () => await _controller.CorrectHouseNumber(
                _backOfficeContext,
                new AddressCorrectHouseNumberRequestValidator(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101" },
                null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "AdresBestaandeHuisnummerBusnummerCombinatie"
                        && e.ErrorMessage == "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam."));
        }

        [Fact]
        public async Task WithCorrectionForBoxNumber_ThenThrowsValidationException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectHouseNumberRequest>(), CancellationToken.None))
                .Throws(new HouseNumberToCorrectHasBoxNumberException());

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            Func<Task> act = async () => await _controller.CorrectHouseNumber(
                _backOfficeContext,
                new AddressCorrectHouseNumberRequestValidator(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101" },
                null);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "AdresCorrectieHuisnummermetBusnummer"
                        && e.ErrorMessage == "Te corrigeren huisnummer mag geen busnummer bevatten."));
        }
    }
}
