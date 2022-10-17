namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressBoxNumber
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressWasProposed : BackOfficeApiTest
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
                .Setup(x => x.Send(It.IsAny<AddressCorrectBoxNumberRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ETagResponse(string.Empty, lastEventHash)));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = (AcceptedWithETagResult) await _controller.CorrectBoxNumber(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectBoxNumberRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectBoxNumberRequest { Busnummer = "101" },
                null, CancellationToken.None);

            //Assert
            result.ETag.Should().Be(lastEventHash);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

           await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = await _controller.CorrectBoxNumber(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectBoxNumberRequest>(),
                MockIfMatchValidator(false),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectBoxNumberRequest { Busnummer = "101" },
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
