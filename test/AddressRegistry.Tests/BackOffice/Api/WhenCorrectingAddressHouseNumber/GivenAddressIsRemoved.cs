namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressHouseNumber
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenThrowsApiException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressCorrectHouseNumberRequest>(), CancellationToken.None))
                .Throws(new AddressIsRemovedException(addressPersistentLocalId));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentIds(addressPersistentLocalId, streetNamePersistentId);

            //Act
            Func<Task> act = async () => await _controller.CorrectHouseNumber(
                _backOfficeContext,
                MockValidRequestValidator<AddressCorrectHouseNumberRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                new AddressCorrectHouseNumberRequest { Huisnummer = "101"},
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                     x.StatusCode == StatusCodes.Status410Gone
                     && x.Message == "Verwijderde adres.");
        }
    }
}
