namespace AddressRegistry.Tests.BackOffice.Api.WhenRemovingAddress
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using StreetName;
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
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
        public async Task ThenAccepted()
        {
            const string lastEventHash = "eventHash";
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressRemoveRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ETagResponse(string.Empty, lastEventHash)));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            var result = await _controller.Remove(
                _backOfficeContext,
                MockValidRequestValidator<AddressRemoveRequest>(),
                MockIfMatchValidator(true),
                null,
                ResponseOptions,
                new AddressRemoveRequest { PersistentLocalId = addressPersistentLocalId });

            //Assert
            result.Should().BeOfType<AcceptedResult>();
            ((AcceptedResult)result).Location.Should().NotBeNull();
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = await _controller.Remove(
                _backOfficeContext,
                MockValidRequestValidator<AddressRemoveRequest>(),
                MockIfMatchValidator(false),
                "IncorrectIfMatchHeader",
                ResponseOptions,
                request: new AddressRemoveRequest { PersistentLocalId = addressPersistentLocalId });

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
