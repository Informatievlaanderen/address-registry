namespace AddressRegistry.Tests.BackOffice.Handlers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenChangingAddressPosition : AddressRegistryBackOfficeTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly TestMunicipalityConsumerContext _municipalityConsumerContext;
        private readonly IStreetNames _streetNames;

        public WhenChangingAddressPosition(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _municipalityConsumerContext = Container.Resolve<TestMunicipalityConsumerContext>();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var municipalityId = Fixture.Create<MunicipalityId>();
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);
            var nisCode = new NisCode("12345");
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentLocalId));
            _backOfficeContext.SaveChanges();

            _municipalityConsumerContext.AddMunicipality(municipalityId, GeometryHelpers.ValidGmlPolygon);

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

            var sut = new AddressChangePositionHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                _streetNames,
                _backOfficeContext,
                _idempotencyContext);

            // Act
            var result = await sut.Handle(new AddressChangePositionRequest
            {
                PersistentLocalId = addressPersistentLocalId,
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = PositieSpecificatie.Gemeente
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 2, 1); //1 = version of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
