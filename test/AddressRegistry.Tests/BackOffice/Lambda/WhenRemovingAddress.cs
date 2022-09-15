namespace AddressRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenRemovingAddress : AddressRegistryBackOfficeTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IStreetNames _streetNames;

        public WhenRemovingAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

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
                new HouseNumber("1"),
                null);

            var etag = new ETagResponse(string.Empty);
            var sut = new SqsAddressRejectHandler(
                MockTicketing(result => { etag = result; }).Object,
                MockTicketingUrl().Object,
                Container.Resolve<ICommandHandlerResolver>(),
                _streetNames,
                _idempotencyContext);

            // Act
            await sut.Handle(
                new SqsAddressRejectRequest
                {
                    PersistentLocalId = addressPersistentLocalId,
                    MessageGroupId = streetNamePersistentLocalId
                },
                CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag.LastEventHash);
        }
    }
}
