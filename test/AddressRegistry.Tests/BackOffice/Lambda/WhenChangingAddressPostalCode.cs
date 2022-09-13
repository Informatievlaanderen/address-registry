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
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenChangingAddressPostalCode : AddressRegistryBackOfficeTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IStreetNames _streetNames;

        public WhenChangingAddressPostalCode(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponse()
        {
            var municipalityId = Fixture.Create<MunicipalityId>();
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);

            var municipalities = Container.Resolve<TestMunicipalityConsumerContext>();
            municipalities.AddMunicipality(municipalityId, GeometryHelpers.ValidGmlPolygon);

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                new NisCode("12345"));

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            ETagResponse? etag = null;

            var sut = new SqsAddressChangePostalCodeHandler(
                MockTicketing(result =>
                {
                    etag = result;
                }).Object,
                MockTicketingUrl().Object,
                Container.Resolve<ICommandHandlerResolver>(),
                _streetNames,
                _idempotencyContext);

            // Act
            await sut.Handle(new SqsAddressChangePostalCodeRequest()
                {
                PersistentLocalId = addressPersistentLocalId,
                MessageGroupId = streetNamePersistentLocalId,
                PostInfoId = $"https://data.vlaanderen.be/id/postinfo/123",
            },
            CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new StreetNameStreamId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag!.LastEventHash);
        }
    }
}
