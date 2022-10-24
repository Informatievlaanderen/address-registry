namespace AddressRegistry.Tests.BackOffice.Handlers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using Infrastructure;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;

    public class WhenCorrectingAddressBoxNumber : BackOfficeHandlerTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IStreetNames _streetNames;

        public WhenCorrectingAddressBoxNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var parentAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var childAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(childAddressPersistentLocalId, streetNamePersistentLocalId);

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                Fixture.Create<NisCode>());

            ProposeAddress(
                streetNamePersistentLocalId,
                parentAddressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("10"),
                null);

            ProposeAddress(
                streetNamePersistentLocalId,
                childAddressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("10"),
                new BoxNumber("1A"));

            var sut = new CorrectAddressBoxNumberHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                _streetNames,
                _backOfficeContext,
                _idempotencyContext);

            // Act
            var result = await sut.Handle(new AddressCorrectBoxNumberRequest
            {
                PersistentLocalId = childAddressPersistentLocalId,
                Busnummer = "20"
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
