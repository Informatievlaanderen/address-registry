namespace AddressRegistry.Tests.BackOffice.Lambda.WhenReaddressingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Autofac;
    using AutoFixture;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenIdempotencyException : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _fakeBackOfficeContext;
        private readonly IStreetNames _streetNames;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;
        private readonly StreetNameStreamId _streetNameStreamId;

        public GivenIdempotencyException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNameId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            _streetNameStreamId = new StreetNameStreamId(_streetNamePersistentLocalId);
        }

        [Fact]
        public async Task GivenRequest_ThenPersistentLocalIdETagResponses()
        {
            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                _streetNamePersistentLocalId,
                new NisCode("12345"));

            var proposeAddress = ProposeAddress(
                _streetNamePersistentLocalId,
                new AddressPersistentLocalId(456),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            var destinationHouseNumber = "13";

            var eTagResponses = new List<ETagResponse>();

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponses = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _fakeBackOfficeContext);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposeAddress.AddressPersistentLocalId,
                _streetNamePersistentLocalId,
                CancellationToken.None);

            var request = new ReaddressLambdaRequest(_streetNamePersistentLocalId, new ReaddressSqsRequest
            {
                Request = new ReaddressRequest()
                {
                    DoelStraatnaamId = StreetNamePuriFor(_streetNamePersistentLocalId),
                    HerAdresseer = new List<AddressToReaddressItem>
                    {
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(proposeAddress.AddressPersistentLocalId),
                            DoelHuisnummer = destinationHouseNumber
                        }
                    }
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            // Act
            await sut.Handle(request, CancellationToken.None);
            await sut.Handle(request, CancellationToken.None);

            // Assert
            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .Find((int) proposeAddress.AddressPersistentLocalId)
                .Should().NotBeNull();

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator always generates with id 1

            eTagResponses.Count.Should().Be(1);
            var destinationAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationAddressPersistentLocalId));
            destinationAddressEtagResponse.Should().NotBeNull();

            var destinationStreetName = await Container.Resolve<IStreetNames>().GetAsync(_streetNameStreamId, CancellationToken.None);
            destinationAddressEtagResponse.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationAddressPersistentLocalId));
        }
    }
}
