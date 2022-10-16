namespace AddressRegistry.Tests.BackOffice.Lambda.WhenCorrectingAddressRetirement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using StreetName;
    using StreetName.Exceptions;
    using AutoFixture;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenRetiringAddress : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IStreetNames _streetNames;

        public WhenRetiringAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithValidHouseNumber());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                Fixture.Create<NisCode>());

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                null);

            ApproveAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId);

            RetireAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, string.Empty);
            var sut = new SqsAddressCorrectRetirementLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponse = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRetirementRequest
            {
                Request = new AddressBackOfficeCorrectRetirementRequest
                {
                    PersistentLocalId = addressPersistentLocalId,
                },
                MessageGroupId = streetNamePersistentLocalId,
                Metadata = new Dictionary<string, object>(),
                TicketId = Guid.NewGuid(),
                Provenance = Fixture.Create<Provenance>()
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 4, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenAddressHasInvalidStatusException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new SqsAddressCorrectRetirementLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRetirementRequest
            {
                Request = new AddressBackOfficeCorrectRetirementRequest
                {
                    PersistentLocalId = Fixture.Create<AddressPersistentLocalId>(),
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
                        "Deze actie is enkel toegestaan op adressen met status 'gehistoreerd'.",
                        "AdresVoorgesteldOfAfgekeurd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressAlreadyExistsException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new SqsAddressCorrectRetirementLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<AddressAlreadyExistsException>().Object);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRetirementRequest
            {
                Request = new AddressBackOfficeCorrectRetirementRequest
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

            var sut = new SqsAddressCorrectRetirementLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<ParentAddressHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRetirementRequest
            {
                Request = new AddressBackOfficeCorrectRetirementRequest
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
                        "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.",
                        "AdresHuisnummerVoorgesteldAfgekeurdOfGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<NisCode>());

            ProposeAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                null);

            RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>());

            var sut = new SqsAddressCorrectRetirementLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                _streetNames,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            var streetName =
                await _streetNames.GetAsync(new StreetNameStreamId(Fixture.Create<StreetNamePersistentLocalId>()), CancellationToken.None);

            // Act
            await sut.Handle(new SqsLambdaAddressCorrectRetirementRequest
            {
                Request = new AddressBackOfficeCorrectRetirementRequest
                {
                    PersistentLocalId = Fixture.Create<AddressPersistentLocalId>(),
                },
                MessageGroupId = Fixture.Create<StreetNamePersistentLocalId>(),
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
                            string.Format(ConfigDetailUrl, Fixture.Create<AddressPersistentLocalId>()),
                            streetName.GetAddressHash(Fixture.Create<AddressPersistentLocalId>()))),
                    CancellationToken.None));
        }
    }
}
