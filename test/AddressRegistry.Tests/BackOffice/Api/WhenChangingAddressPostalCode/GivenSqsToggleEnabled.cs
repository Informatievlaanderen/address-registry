namespace AddressRegistry.Tests.BackOffice.Api.WhenChangingAddressPostalCode
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled  : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>(useSqs: true);
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketLocation = Fixture.Create<Uri>();
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<SqsAddressChangePostalCodeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new LocationResult(ticketLocation)));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            var result = (AcceptedResult)await _controller.ChangePostalCode(
                _backOfficeContext,
                MockValidRequestValidator<AddressChangePostalCodeRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                addressPersistentLocalId,
                request: new AddressChangePostalCodeRequest
                {
                    PostInfoId = PostInfoPuri
                },
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            result.Location.Should().Be(ticketLocation.ToString());
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
                request: new AddressChangePostalCodeRequest
                {
                    PostInfoId = PostInfoPuri
                },
                ifMatchHeaderValue: null);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task ForUnknownAddress_ThenNotFoundResponse()
        {
            var result = await _controller.ChangePostalCode(
                _backOfficeContext,
                MockValidRequestValidator<AddressChangePostalCodeRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                Fixture.Create<AddressPersistentLocalId>(),
                request: new AddressChangePostalCodeRequest
                {
                    PostInfoId = PostInfoPuri
                },
                ifMatchHeaderValue: null);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
