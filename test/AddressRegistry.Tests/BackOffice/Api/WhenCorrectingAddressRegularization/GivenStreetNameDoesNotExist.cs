namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressRegularization
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenStreetNameDoesNotExist : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenStreetNameDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public void WhenAggregateIdIsNotFoundException_ThenApiException()
        {
            //Arrange
            var streetNamePersistentId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            MockMediator.Setup(x => x.Send(It.IsAny<CorrectAddressRegularizationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            //Act
            Func<Task> act = async () => await _controller.CorrectRegularization(
                _backOfficeContext,
                MockIfMatchValidator(true),
                null,
                new AddressPersistentLocalId(456),
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == $"De straatnaam '{streetNamePersistentId}' is niet gekend in het straatnaamregister.");
        }

        [Fact]
        public void WhenAggregateNotFoundException_ThenApiException()
        {
            //Arrange
            var streetNamePersistentId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            MockMediator.Setup(x => x.Send(It.IsAny<CorrectAddressRegularizationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateNotFoundException(streetNamePersistentId, typeof(string)));

            //Act
            Func<Task> act = async () => await _controller.CorrectRegularization(
                _backOfficeContext,
                MockIfMatchValidator(true),
                null,
                new AddressPersistentLocalId(456),
                CancellationToken.None);

            //Assert
            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == $"De straatnaam '{streetNamePersistentId}' is niet gekend in het straatnaamregister.");
        }
    }
}
