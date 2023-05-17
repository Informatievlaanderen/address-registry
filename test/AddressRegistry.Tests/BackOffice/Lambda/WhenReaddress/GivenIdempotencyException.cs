namespace AddressRegistry.Tests.BackOffice.Lambda.WhenReaddress
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
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AddressRegistry.Tests.AutoFixture;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using AddressRegistry.Tests.BackOffice.Lambda.Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
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

            var houseNumber = new HouseNumber("11");
            var proposeHouseNumberAddress = await ProposeAddress(new AddressPersistentLocalId(123), houseNumber);
            var proposeBoxNumber = await ProposeAddress(new AddressPersistentLocalId(456), houseNumber, new BoxNumber("A1"));
            var proposeBoxNumberToApprove = await ProposeAddress(new AddressPersistentLocalId(789), houseNumber, new BoxNumber("A2"));
            ApproveAddress(_streetNamePersistentLocalId, new AddressPersistentLocalId(proposeHouseNumberAddress.AddressPersistentLocalId));
            ApproveAddress(_streetNamePersistentLocalId, new AddressPersistentLocalId(proposeBoxNumberToApprove.AddressPersistentLocalId));

            var destinationHouseNumber = "13";

            var eTagResponses = new List<ETagResponse>();

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponses = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _fakeBackOfficeContext,
                Container);

            var request = new ReaddressLambdaRequest(_streetNamePersistentLocalId, new ReaddressSqsRequest
            {
                Request = new ReaddressRequest
                {
                    DoelStraatnaamId = StreetNamePuriFor(_streetNamePersistentLocalId),
                    HerAdresseer = new List<AddressToReaddressItem>
                    {
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(proposeHouseNumberAddress.AddressPersistentLocalId),
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
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator always starts with id 1

            (await _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                    .FindAsync((int) destinationAddressPersistentLocalId)).Should().NotBeNull();

            eTagResponses.Count.Should().Be(1);
            var destinationAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationAddressPersistentLocalId));
            destinationAddressEtagResponse.Should().NotBeNull();

            var destinationStreetName = await Container.Resolve<IStreetNames>().GetAsync(_streetNameStreamId, CancellationToken.None);
            destinationAddressEtagResponse.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationAddressPersistentLocalId));
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            base.ConfigureCommandHandling(builder);

            builder.Register(c =>
                    new IdempotentCommandHandler(c.Resolve<ICommandHandlerResolver>(), _idempotencyContext))
                .As<IIdempotentCommandHandler>();
        }

        [Fact]
        public async Task GivenRequestWithToRetireAddressesAndDifferentStreetNamesAndSameHouseNumber_ThenPersistentLocalIdETagResponses()
        {
            var sourceStreetNamePersistentLocalId = _streetNamePersistentLocalId;
            var sourceStreamId = _streetNameStreamId;
            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                sourceStreetNamePersistentLocalId,
                new NisCode("12345"));

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(12345);
            var destinationStreamId = new StreetNameStreamId(destinationStreetNamePersistentLocalId);
            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                destinationStreetNamePersistentLocalId,
                new NisCode("12345"));

            var houseNumber = new HouseNumber("11");

            var sourceHouseNumberAddress = await ProposeAddress(new AddressPersistentLocalId(123), houseNumber);
            var proposeBoxNumberToApprove = await ProposeAddress(new AddressPersistentLocalId(789), houseNumber, new BoxNumber("A1"));
            ApproveAddress(_streetNamePersistentLocalId, new AddressPersistentLocalId(sourceHouseNumberAddress.AddressPersistentLocalId));
            ApproveAddress(_streetNamePersistentLocalId, new AddressPersistentLocalId(proposeBoxNumberToApprove.AddressPersistentLocalId));

            var eTagResponses = new List<ETagResponse>();

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponses = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _fakeBackOfficeContext,
                Container);

            var request = new ReaddressLambdaRequest(destinationStreetNamePersistentLocalId, new ReaddressSqsRequest
            {
                Request = new ReaddressRequest
                {
                    DoelStraatnaamId = StreetNamePuriFor(destinationStreetNamePersistentLocalId),
                    HerAdresseer = new List<AddressToReaddressItem>
                    {
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(sourceHouseNumberAddress.AddressPersistentLocalId),
                            DoelHuisnummer = houseNumber
                        }
                    },
                    OpheffenAdressen = new List<string>(){AddressPuriFor(sourceHouseNumberAddress.AddressPersistentLocalId)}
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            // Act
            await sut.Handle(request, CancellationToken.None);
            await sut.Handle(request, CancellationToken.None);

            // Assert
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator always starts with id 1

            (await _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                    .FindAsync((int) destinationAddressPersistentLocalId)).Should().NotBeNull();

            eTagResponses.Count.Should().Be(2);

            var sourceAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, sourceHouseNumberAddress.AddressPersistentLocalId));
            sourceAddressEtagResponse.Should().NotBeNull();

            var destinationAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationAddressPersistentLocalId));
            destinationAddressEtagResponse.Should().NotBeNull();

            var sourceStreetName = await Container.Resolve<IStreetNames>().GetAsync(sourceStreamId, CancellationToken.None);
            sourceAddressEtagResponse.ETag.Should().Be(sourceStreetName.GetAddressHash(sourceHouseNumberAddress.AddressPersistentLocalId));

            var destinationStreetName = await Container.Resolve<IStreetNames>().GetAsync(destinationStreamId, CancellationToken.None);
            destinationAddressEtagResponse.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationAddressPersistentLocalId));
        }

        private async Task<ProposeAddress> ProposeAddress(
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber = null)
        {
            var proposeAddress = ProposeAddress(
                _streetNamePersistentLocalId,
                addressPersistentLocalId,
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                boxNumber);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposeAddress.AddressPersistentLocalId,
                _streetNamePersistentLocalId,
                CancellationToken.None);

            return proposeAddress;
        }
    }
}
