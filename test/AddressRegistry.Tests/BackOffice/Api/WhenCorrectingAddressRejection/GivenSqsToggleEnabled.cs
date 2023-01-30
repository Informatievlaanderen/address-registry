namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressRejection
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled  : BackOfficeApiTest
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
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressRejectionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            var result = (AcceptedResult)await _controller.CorrectRejection(
                _backOfficeContext,
                MockValidRequestValidator<CorrectAddressRejectionRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                request: new CorrectAddressRejectionRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            //Act
            var result = await _controller.CorrectRejection(
                _backOfficeContext,
                MockValidRequestValidator<CorrectAddressRejectionRequest>(),
                MockIfMatchValidator(false),
                ResponseOptions,
                request: new CorrectAddressRejectionRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                ifMatchHeaderValue: null);

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task ForUnknownAddress_ThenThrowsApiException()
        {
            Func<Task> act = async () => await _controller.CorrectRejection(
                _backOfficeContext,
                MockValidRequestValidator<CorrectAddressRejectionRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                new CorrectAddressRejectionRequest
                {
                    PersistentLocalId = Fixture.Create<AddressPersistentLocalId>()
                },
                ifMatchHeaderValue: null);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand adres.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectAddressRejectionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await _controller.CorrectRejection(
                _backOfficeContext,
                MockValidRequestValidator<CorrectAddressRejectionRequest>(),
                MockIfMatchValidator(true),
                ResponseOptions,
                new CorrectAddressRejectionRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                string.Empty);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand adres.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }
    }
}
