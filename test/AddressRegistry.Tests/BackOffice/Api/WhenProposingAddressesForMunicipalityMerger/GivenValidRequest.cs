namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddressesForMunicipalityMerger
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Consumer.Read.StreetName.Projections;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenValidRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenValidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void ThenTicketLocationsAreReturned()
        {
            var persistentLocalIdOne = Fixture.Create<PersistentLocalId>();
            var persistentLocalIdTwo = Fixture.Create<PersistentLocalId>();
            var persistentLocalIdThree = Fixture.Create<PersistentLocalId>();
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .SetupSequence(x => x.GenerateNextPersistentLocalId())
                .Returns(persistentLocalIdOne)
                .Returns(persistentLocalIdTwo)
                .Returns(persistentLocalIdThree);

            var ticketIdOne = Fixture.Create<Guid>();
            var expectedLocationResultOne = new LocationResult(CreateTicketUri(ticketIdOne));
            var ticketIdTwo = Fixture.Create<Guid>();
            var expectedLocationResultTwo = new LocationResult(CreateTicketUri(ticketIdTwo));
            MockMediator
                .SetupSequence(x => x.Send(It.IsAny<ProposeAddressesForMunicipalityMergerSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResultOne))
                .Returns(Task.FromResult(expectedLocationResultTwo));

            var newStreetNamePersistentLocalIdOne = Fixture.Create<PersistentLocalId>();
            var newStreetNamePersistentLocalIdTwo = Fixture.Create<PersistentLocalId>();
            const string nisCode = "10000";
            const string newStreetNameNameOne = "Vagevuurstraat";
            const string newStreetNameHomonymAdditionOne = "(HO)";
            const string newStreetNameNameTwo = "Molendorpstraat";

            var streetNameConsumerContext = new FakeStreetNameConsumerContextFactory().CreateDbContext();
            streetNameConsumerContext.StreetNameLatestItems.Add(new StreetNameLatestItem(
                newStreetNamePersistentLocalIdOne,
                nisCode)
            {
                NameDutch = newStreetNameNameOne,
                HomonymAdditionDutch = newStreetNameHomonymAdditionOne
            });
            streetNameConsumerContext.StreetNameLatestItems.Add(new StreetNameLatestItem(
                newStreetNamePersistentLocalIdTwo,
                nisCode)
            {
                NameDutch = newStreetNameNameTwo
            });
            streetNameConsumerContext.SaveChanges();

            const int oldStreetNamePersistenLocalIdOne = 59111;
            const int oldStreetNamePersistenLocalIdTwo = 59112;
            const int oldAddressPersistenLocalIdOne = 2268196;
            const int oldAddressPersistenLocalIdTwo = 2268197;
            const int oldAddressPersistenLocalIdThree = 2268198;
            const string houseNumberOne = "14";
            const string houseNumberTwo = "30";
            const string boxNumber = "b";
            const string postalCode = "8755";
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString($"""
                                                         OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
                                                         https://data.vlaanderen.be/id/straatnaam/{oldStreetNamePersistenLocalIdOne};https://data.vlaanderen.be/id/adres/{oldAddressPersistenLocalIdOne};{newStreetNameNameOne};{newStreetNameHomonymAdditionOne};{houseNumberOne};;{postalCode}
                                                         https://data.vlaanderen.be/id/straatnaam/{oldStreetNamePersistenLocalIdOne};https://data.vlaanderen.be/id/adres/{oldAddressPersistenLocalIdTwo};{newStreetNameNameOne};{newStreetNameHomonymAdditionOne};{houseNumberOne};{boxNumber};{postalCode}
                                                         https://data.vlaanderen.be/id/straatnaam/{oldStreetNamePersistenLocalIdTwo};https://data.vlaanderen.be/id/adres/{oldAddressPersistenLocalIdThree};{newStreetNameNameTwo};;{houseNumberTwo};;{postalCode}
                                                         """),
                    nisCode,
                    persistentLocalIdGenerator.Object,
                    streetNameConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(new[]
            {
                ticketIdOne, ticketIdTwo
            }.Select(x => $"{PublicTicketUrl}/tickets/{x:D}"));

            MockMediator.Verify(x =>
                x.Send(It.IsAny<ProposeAddressesForMunicipalityMergerSqsRequest>(), CancellationToken.None), Times.Exactly(2));

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeAddressesForMunicipalityMergerSqsRequest>(sqsRequest =>
                        sqsRequest.StreetNamePersistentLocalId == newStreetNamePersistentLocalIdOne
                        && sqsRequest.Addresses.Count == 2
                        && sqsRequest.Addresses.First().AddressPersistentLocalId == persistentLocalIdOne
                        && sqsRequest.Addresses.First().MergedAddressPersistentLocalId == oldAddressPersistenLocalIdOne
                        && sqsRequest.Addresses.First().MergedStreetNamePersistentLocalId == oldStreetNamePersistenLocalIdOne
                        && sqsRequest.Addresses.First().HouseNumber == houseNumberOne
                        && sqsRequest.Addresses.First().BoxNumber == null
                        && sqsRequest.Addresses.First().PostalCode == postalCode
                        && sqsRequest.Addresses.Last().AddressPersistentLocalId == persistentLocalIdTwo
                        && sqsRequest.Addresses.Last().MergedAddressPersistentLocalId == oldAddressPersistenLocalIdTwo
                        && sqsRequest.Addresses.Last().MergedStreetNamePersistentLocalId == oldStreetNamePersistenLocalIdOne
                        && sqsRequest.Addresses.Last().HouseNumber == houseNumberOne
                        && sqsRequest.Addresses.Last().BoxNumber == boxNumber
                        && sqsRequest.Addresses.Last().PostalCode == postalCode
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeAddressesForMunicipalityMergerSqsRequest>(sqsRequest =>
                        sqsRequest.StreetNamePersistentLocalId == newStreetNamePersistentLocalIdTwo
                        && sqsRequest.Addresses.Count == 1
                        && sqsRequest.Addresses.First().AddressPersistentLocalId == persistentLocalIdThree
                        && sqsRequest.Addresses.First().MergedAddressPersistentLocalId == oldAddressPersistenLocalIdThree
                        && sqsRequest.Addresses.First().MergedStreetNamePersistentLocalId == oldStreetNamePersistenLocalIdTwo
                        && sqsRequest.Addresses.First().HouseNumber == houseNumberTwo
                        && sqsRequest.Addresses.First().BoxNumber == null
                        && sqsRequest.Addresses.First().PostalCode == postalCode
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));
        }
    }
}
