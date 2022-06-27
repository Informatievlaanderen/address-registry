namespace AddressRegistry.Tests.BackOffice.Api.WhenApprovingAddress
{
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Address;
    using AddressRegistry.Api.BackOffice.Address.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressWasProposed : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenAddressWasProposed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task ThenNoContentWithETagResultIsReturned()
        {
            // Arrange
            var streetNameId = Fixture.Create<StreetNameId>();
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            ImportMigratedStreetName(streetNameId, streetNamePersistentId);
            ProposeAddress(
                streetNamePersistentId,
                addressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(
                    addressPersistentLocalId,
                    streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            var result = (NoContentWithETagResult)await _controller.Approve(
                _idempotencyContext,
                _backOfficeContext,
                new AddressApproveRequestValidator(),
            Container.Resolve<IStreetNames>(),
                addressApproveRequest: new AddressApproveRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                ifMatchHeaderValue: null);

            var stream = await Container
                .Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new StreetNameStreamId(streetNamePersistentId)), 2, 1); //2 = version of stream (zero based)

            //Assert
            result.ETag.Length.Should().Be(128);
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
