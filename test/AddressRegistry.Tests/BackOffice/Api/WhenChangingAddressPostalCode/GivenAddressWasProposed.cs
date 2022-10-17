namespace AddressRegistry.Tests.BackOffice.Api.WhenChangingAddressPostalCode
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetName;
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
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
                .Setup(x => x.Send(It.IsAny<AddressChangePostalCodeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ETagResponse(string.Empty, lastEventHash)));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = (AcceptedWithETagResult) await _controller.ChangePostalCode(
                _backOfficeContext,
                MockValidRequestValidator<AddressChangePostalCodeRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressChangePostalCodeRequest
                {
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/456",
                },
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
            var result = await _controller.ChangePostalCode(
                _backOfficeContext,
                MockValidRequestValidator<AddressChangePostalCodeRequest>(),
                MockIfMatchValidator(false),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressChangePostalCodeRequest
                {
                    PostInfoId = $"https://data.vlaanderen.be/id/postinfo/456",
                },
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
