namespace AddressRegistry.Tests.BackOffice.Lambda.WhenCorrectingAddressRejection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Exceptions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Autofac;
    using AutoFixture;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IStreetNames _streetNames;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);
            var nisCode = new NisCode("12345");
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

            var rejectAddress = new RejectAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(rejectAddress);

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var sut = new SqsAddressCorrectRejectionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRejectionRequest
            {
                Request = new AddressBackOfficeCorrectAddressFromRejectedRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                MessageGroupId = streetNamePersistentLocalId,
                Metadata = new Dictionary<string, object>(),
                TicketId = Guid.NewGuid(),
                Provenance = Fixture.Create<Provenance>()
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenAddressHasInvalidStatusException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new SqsAddressCorrectRejectionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRejectionRequest
            {
                Request = new AddressBackOfficeCorrectAddressFromRejectedRequest
                {
                    PersistentLocalId = Fixture.Create<int>(),
                },
                MessageGroupId = Fixture.Create<int>().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op een adres met status 'afgekeurd'.",
                        "AdresInGebruikOfGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressAlreadyExistsException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new SqsAddressCorrectRejectionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRejectionRequest
            {
                Request = new AddressBackOfficeCorrectAddressFromRejectedRequest
                {
                    PersistentLocalId = Fixture.Create<int>(),
                },
                MessageGroupId = Fixture.Create<int>().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.",
                        "AdresBestaandeHuisnummerBusnummerCombinatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenParentAddressHasInvalidStatusException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new SqsAddressCorrectRejectionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<ParentAddressHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRejectionRequest
            {
                Request = new AddressBackOfficeCorrectAddressFromRejectedRequest
                {
                    PersistentLocalId = Fixture.Create<int>(),
                },
                MessageGroupId = Fixture.Create<int>().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'voorgesteld' of 'inGebruik' heeft.",
                        "AdresHuisnummerAfgekeurdOfGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);
            var nisCode = new NisCode("12345");
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

            var regularizeAddress = new CorrectAddressRejection(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(regularizeAddress);

            var sut = new SqsAddressCorrectRejectionLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                _streetNames,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            var streetName =
                await _streetNames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), CancellationToken.None);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRejectionRequest
            {
                Request = new AddressBackOfficeCorrectAddressFromRejectedRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                MessageGroupId = streetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            },
            CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, addressPersistentLocalId),
                            streetName.GetAddressHash(addressPersistentLocalId))),
                    CancellationToken.None));
        }
    }
}
