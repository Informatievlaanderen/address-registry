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
    using Autofac;
    using AutoFixture;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using StreetName;
    using StreetName.Commands;
    using Xunit;
    using Xunit.Abstractions;
    using IdempotentCommandHandler = Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers.IdempotentCommandHandler;
    using IIdempotentCommandHandler = Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers.IIdempotentCommandHandler;

    public class GivenStreetNameExists : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _fakeBackOfficeContext;
        private readonly IStreetNames _streetNames;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;
        private readonly StreetNameStreamId _streetNameStreamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNameId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _streetNames = Container.Resolve<IStreetNames>();
            _streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            _streetNameStreamId = new StreetNameStreamId(_streetNamePersistentLocalId);
        }

        [Fact]
        public async Task WithRequestWithOneNoneExistingDestination_ThenPersistentLocalIdETagResponses()
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
                _fakeBackOfficeContext,
                Container);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposeAddress.AddressPersistentLocalId,
                _streetNamePersistentLocalId,
                CancellationToken.None);

            // Act
            await sut.Handle(new ReaddressLambdaRequest(_streetNamePersistentLocalId, new ReaddressSqsRequest
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
            }),
            CancellationToken.None);

            // Assert
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts always with id 1

            var relation = _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
            relation.Should().NotBeNull();


            eTagResponses.Count.Should().Be(1);
            var destinationAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationAddressPersistentLocalId));
            destinationAddressEtagResponse.Should().NotBeNull();

            var destinationStreetName = await Container.Resolve<IStreetNames>().GetAsync(_streetNameStreamId, CancellationToken.None);
            destinationAddressEtagResponse!.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationAddressPersistentLocalId));
        }

        [Fact]
        public async Task WithRequestWithTwoNoneExistingDestinations_ThenPersistentLocalIdETagResponses()
        {
            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                _streetNamePersistentLocalId,
                new NisCode("12345"));

            var proposeAddress1 = ProposeAddress(
                _streetNamePersistentLocalId,
                new AddressPersistentLocalId(456),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposeAddress1.AddressPersistentLocalId,
                _streetNamePersistentLocalId,
                CancellationToken.None);

            var proposeAddress2 = ProposeAddress(
                _streetNamePersistentLocalId,
                new AddressPersistentLocalId(789),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("12"),
                null);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposeAddress2.AddressPersistentLocalId,
                _streetNamePersistentLocalId,
                CancellationToken.None);

            var destinationHouseNumber1 = "13";
            var destinationHouseNumber2 = "14";

            var eTagResponses = new List<ETagResponse>();

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponses = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _fakeBackOfficeContext,
                Container);

            // Act
            await sut.Handle(new ReaddressLambdaRequest(_streetNamePersistentLocalId, new ReaddressSqsRequest
            {
                Request = new ReaddressRequest()
                {
                    DoelStraatnaamId = StreetNamePuriFor(_streetNamePersistentLocalId),
                    HerAdresseer = new List<AddressToReaddressItem>
                    {
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(proposeAddress1.AddressPersistentLocalId),
                            DoelHuisnummer = destinationHouseNumber1
                        },
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(proposeAddress2.AddressPersistentLocalId),
                            DoelHuisnummer = destinationHouseNumber2
                        }
                    }
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }),
            CancellationToken.None);

            // Assert
            var destinationAddressPersistentLocalId1 = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts always with id 1
            var destinationAddressPersistentLocalId2 = new AddressPersistentLocalId(2);

            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId1)
                .Should().NotBeNull();
            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId2)
               .Should().NotBeNull();

            eTagResponses.Count.Should().Be(2);

            var destinationAddressEtagResponse1 = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationAddressPersistentLocalId1));
            destinationAddressEtagResponse1.Should().NotBeNull();

            var destinationAddressEtagResponse2 = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationAddressPersistentLocalId2));
            destinationAddressEtagResponse2.Should().NotBeNull();

            var destinationStreetName = await Container.Resolve<IStreetNames>().GetAsync(_streetNameStreamId, CancellationToken.None);

            destinationAddressEtagResponse1!.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationAddressPersistentLocalId1));
            destinationAddressEtagResponse2!.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationAddressPersistentLocalId2));
        }

        [Fact]
        public async Task WithSourceAddressWithBoxNumbersAndNonExistingDestinationAddress_ThenPersistentLocalIdETagResponses()
        {
            ImportMigratedStreetName(new StreetNameId(Guid.NewGuid()), _streetNamePersistentLocalId, new NisCode("12345"));

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

            // Act
            await sut.Handle(new ReaddressLambdaRequest(_streetNamePersistentLocalId, new ReaddressSqsRequest
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
            }),
            CancellationToken.None);

            // Assert
            var destinationHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts always with id 1
            var destinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            (await _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                    .FindAsync((int)destinationHouseNumberAddressPersistentLocalId)).Should().NotBeNull();
            (await _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                    .FindAsync((int)destinationProposedBoxNumberAddressPersistentLocalId)).Should().NotBeNull();
            (await _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                    .FindAsync((int)destinationCurrentBoxNumberAddressPersistentLocalId)).Should().NotBeNull();

            eTagResponses.Count.Should().Be(1);

            var destinationAddressEtagResponse = eTagResponses
                .FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, destinationHouseNumberAddressPersistentLocalId));
            destinationAddressEtagResponse.Should().NotBeNull();

            var destinationStreetName = await Container.Resolve<IStreetNames>().GetAsync(_streetNameStreamId, CancellationToken.None);

            destinationAddressEtagResponse!.ETag.Should().Be(destinationStreetName.GetAddressHash(destinationHouseNumberAddressPersistentLocalId));
        }

        [Fact]
        public async Task WithAddressesToRetireOutsideDestinationStreetName_ThenPersistentLocalIdETagResponses()
        {
            var firstOtherStreetNamePersistentLocalId = new StreetNamePersistentLocalId(456);
            var secondOtherStreetNamePersistentLocalId = new StreetNamePersistentLocalId(789);

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                _streetNamePersistentLocalId,
                new NisCode("12345"));

            ImportMigratedStreetName(
              new StreetNameId(Guid.NewGuid()),
              firstOtherStreetNamePersistentLocalId,
              new NisCode("12345"));

            ImportMigratedStreetName(
              new StreetNameId(Guid.NewGuid()),
              secondOtherStreetNamePersistentLocalId,
              new NisCode("12345"));

            var proposedAddressOfFirstStreetName = ProposeAddress(
                firstOtherStreetNamePersistentLocalId,
                new AddressPersistentLocalId(456),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            var proposedAddressOfSecondStreetName = ProposeAddress(
                secondOtherStreetNamePersistentLocalId,
                new AddressPersistentLocalId(789),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                null);

            var proposedBoxNumberAddressOfSecondStreetName = ProposeAddress(
                secondOtherStreetNamePersistentLocalId,
                new AddressPersistentLocalId(7891),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("11"),
                new BoxNumber("A"));

            var anotherProposedAddressOfSecondStreetName = ProposeAddress(
                secondOtherStreetNamePersistentLocalId,
                new AddressPersistentLocalId(7892),
                new PostalCode("2018"),
                Fixture.Create<MunicipalityId>(),
                new HouseNumber("9"),
                null);

            var destinationHouseNumber = "13";
            var secondDestinationHouseNumber = "15";
            var thirdDestinationHouseNumber = "17";

            var eTagResponses = new List<ETagResponse>();

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { eTagResponses = result; }).Object,
                _streetNames,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _fakeBackOfficeContext,
                Container);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposedAddressOfFirstStreetName.AddressPersistentLocalId,
                firstOtherStreetNamePersistentLocalId,
                CancellationToken.None);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposedAddressOfSecondStreetName.AddressPersistentLocalId,
                secondOtherStreetNamePersistentLocalId,
                CancellationToken.None);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                proposedBoxNumberAddressOfSecondStreetName.AddressPersistentLocalId,
                secondOtherStreetNamePersistentLocalId,
                CancellationToken.None);

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                anotherProposedAddressOfSecondStreetName.AddressPersistentLocalId,
                secondOtherStreetNamePersistentLocalId,
                CancellationToken.None);

            // Act
            await sut.Handle(new ReaddressLambdaRequest(_streetNamePersistentLocalId, new ReaddressSqsRequest
            {
                Request = new ReaddressRequest
                {
                    DoelStraatnaamId = StreetNamePuriFor(_streetNamePersistentLocalId),
                    HerAdresseer = new List<AddressToReaddressItem>
                    {
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(proposedAddressOfFirstStreetName.AddressPersistentLocalId),
                            DoelHuisnummer = destinationHouseNumber
                        },
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(proposedAddressOfSecondStreetName.AddressPersistentLocalId),
                            DoelHuisnummer = secondDestinationHouseNumber
                        },
                        new AddressToReaddressItem
                        {
                            BronAdresId = AddressPuriFor(anotherProposedAddressOfSecondStreetName.AddressPersistentLocalId),
                            DoelHuisnummer = thirdDestinationHouseNumber
                        }
                    },
                    OpheffenAdressen = new List<string>()
                    {
                        AddressPuriFor(proposedAddressOfFirstStreetName.AddressPersistentLocalId),
                        AddressPuriFor(proposedAddressOfSecondStreetName.AddressPersistentLocalId),
                        AddressPuriFor(anotherProposedAddressOfSecondStreetName.AddressPersistentLocalId),
                    }
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }),
            CancellationToken.None);

            // Assert
            var firstDestinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts always with id 1
            var secondDestinationAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondDestinationBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var thirdDestinationBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(4);

            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == firstDestinationAddressPersistentLocalId)
                .Should().NotBeNull();

            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == secondDestinationAddressPersistentLocalId)
               .Should().NotBeNull();

            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == secondDestinationBoxNumberAddressPersistentLocalId)
               .Should().NotBeNull();

            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == thirdDestinationBoxNumberAddressPersistentLocalId)
               .Should().NotBeNull();

            eTagResponses.Count.Should().Be(6);
            var firstDestinationAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, firstDestinationAddressPersistentLocalId));
            firstDestinationAddressEtagResponse.Should().NotBeNull();
            var secondDestinationAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, secondDestinationAddressPersistentLocalId));
            secondDestinationAddressEtagResponse.Should().NotBeNull();

            var firstRejectedAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, proposedAddressOfFirstStreetName.AddressPersistentLocalId));
            firstRejectedAddressEtagResponse.Should().NotBeNull();
            var secondRejectedAddressEtagResponse = eTagResponses.FirstOrDefault(x => x.Location == string.Format(ConfigDetailUrl, proposedAddressOfSecondStreetName.AddressPersistentLocalId));
            secondRejectedAddressEtagResponse.Should().NotBeNull();

            var lifeTimeScope = Container.Resolve<ILifetimeScope>();
            await using var scope = lifeTimeScope.BeginLifetimeScope();

            var destinationStreetName = await scope.Resolve<IStreetNames>().GetAsync(_streetNameStreamId, CancellationToken.None);
            firstDestinationAddressEtagResponse!.ETag.Should().Be(destinationStreetName.GetAddressHash(firstDestinationAddressPersistentLocalId));
            secondDestinationAddressEtagResponse!.ETag.Should().Be(destinationStreetName.GetAddressHash(secondDestinationAddressPersistentLocalId));

            var firstSourceStreetName = await scope.Resolve<IStreetNames>().GetAsync(new StreetNameStreamId(firstOtherStreetNamePersistentLocalId), CancellationToken.None);
            firstRejectedAddressEtagResponse!.ETag.Should().Be(firstSourceStreetName.GetAddressHash(proposedAddressOfFirstStreetName.AddressPersistentLocalId));
            var secondSourceStreetName = await scope.Resolve<IStreetNames>().GetAsync(new StreetNameStreamId(secondOtherStreetNamePersistentLocalId), CancellationToken.None);
            secondRejectedAddressEtagResponse!.ETag.Should().Be(secondSourceStreetName.GetAddressHash(proposedAddressOfSecondStreetName.AddressPersistentLocalId));
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

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            base.ConfigureCommandHandling(builder);

            builder.Register(c => new IdempotentCommandHandler(c.Resolve<ICommandHandlerResolver>(), _idempotencyContext))
                .As<IIdempotentCommandHandler>();
        }
    }
}
