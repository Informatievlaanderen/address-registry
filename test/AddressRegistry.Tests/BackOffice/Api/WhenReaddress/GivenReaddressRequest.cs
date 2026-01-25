namespace AddressRegistry.Tests.BackOffice.Api.WhenReaddress
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenReaddressRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenReaddressRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<ReaddressSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<ReaddressRequest>();

            var result = (AcceptedResult)await _controller.Readdress(
                MockValidRequestValidator<ReaddressRequest>(),
                request);

            // Assert
            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ReaddressSqsRequest>(sqsRequest =>
                            sqsRequest.Request == request
                            && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                            && sqsRequest.ProvenanceData.Application == Application.AddressRegistry
                            && sqsRequest.ProvenanceData.Modification == Modification.Update
                            && string.IsNullOrEmpty(sqsRequest.IfMatchHeaderValue)
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public void WithNonExistingStreetName_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: false);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();

            var request = new ReaddressRequest
            {
                DoelStraatnaamId = $"{StraatNaamPuri}/1",
                HerAdresseer = new List<AddressToReaddressItem>()
            };

            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                request);

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorMessage == $"De straatnaam '{request.DoelStraatnaamId}' is niet gekend in het straatnaamregister." &&
                    e.ErrorCode == "AdresStraatnaamNietGekendValidatie"));
        }

        [Fact]
        public void WithEmptyHernummerAdressen_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: true);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();

            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                new ReaddressRequest
                {
                    DoelStraatnaamId = $"{StraatNaamPuri}/1",
                    HerAdresseer = new List<AddressToReaddressItem>()
                });

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorMessage == "De lijst van te heradresseren adressen kan niet leeg zijn."
                        && e.ErrorCode == "HerAdresseerLijstLeeg"));
        }

        [Fact]
        public async Task WithNonExistingAddressesToReaddress_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: true);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var existingAdddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAdddressPersistentLocalId, Fixture.Create<StreetNamePersistentLocalId>());

            var nonExistingAddres1 = $"{AdresPuri}{existingAdddressPersistentLocalId + 1}";
            var invaliPuri = $"{existingAdddressPersistentLocalId + 2}";
            var request = new ReaddressRequest
            {
                DoelStraatnaamId = $"{StraatNaamPuri}/1",
                HerAdresseer = new List<AddressToReaddressItem>
                {
                    new AddressToReaddressItem {BronAdresId = $"{AdresPuri}{existingAdddressPersistentLocalId}", DoelHuisnummer = "1"},
                    new AddressToReaddressItem {BronAdresId = nonExistingAddres1, DoelHuisnummer = "3"},
                    new AddressToReaddressItem {BronAdresId = invaliPuri, DoelHuisnummer = "5"},
                }
            };
            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                request);

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorMessage == $"Onbestaand adres '{nonExistingAddres1}'." && e.ErrorCode == "AdresIsOnbestaand")
                    && x.Errors.Any(e => e.ErrorMessage == $"Onbestaand adres '{invaliPuri}'." && e.ErrorCode == "AdresIsOnbestaand"));
        }

        [Fact]
        public async Task WithInvalidHouseNumber_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: true);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var existingAdddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAdddressPersistentLocalId, Fixture.Create<StreetNamePersistentLocalId>());

            var request = new ReaddressRequest
            {
                DoelStraatnaamId = $"{StraatNaamPuri}/1",
                HerAdresseer = new List<AddressToReaddressItem>
                {
                    new AddressToReaddressItem
                    {
                        BronAdresId = $"{AdresPuri}{existingAdddressPersistentLocalId}",
                        DoelHuisnummer = "A"
                    }
                }
            };

            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                request);

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorMessage == "Ongeldig huisnummerformaat: A."
                        && e.ErrorCode == "DoelHuisnummerOngeldigHuisnummerformaat"));
        }

        [Fact]
        public async Task WithAddressToRetireNotInAddressToReaddress_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: true);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var existingAdddressPersistentLocalId1 = Fixture.Create<AddressPersistentLocalId>();
            var existingAdddressPersistentLocalId2 = new AddressPersistentLocalId(existingAdddressPersistentLocalId1 + 1);
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAdddressPersistentLocalId1, Fixture.Create<StreetNamePersistentLocalId>());
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAdddressPersistentLocalId2, Fixture.Create<StreetNamePersistentLocalId>());

            var request = new ReaddressRequest
            {
                DoelStraatnaamId = $"{StraatNaamPuri}/1",
                HerAdresseer = new List<AddressToReaddressItem>
                {
                    new AddressToReaddressItem {BronAdresId = $"{AdresPuri}{existingAdddressPersistentLocalId1}", DoelHuisnummer = "1"}
                },
                OpheffenAdressen = new List<string> { $"{AdresPuri}{existingAdddressPersistentLocalId1}", $"{AdresPuri}{existingAdddressPersistentLocalId2}" }
            };
            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                request);

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorMessage == "Het op te heffen adres dient voor te komen in de lijst van herAdresseer."
                        && e.ErrorCode == "OpgehevenAdresNietInLijstHerAdresseer"));
        }

        [Fact]
        public async Task WithDuplicateSourceAddressIds_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: true);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var existingAdddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAdddressPersistentLocalId, Fixture.Create<StreetNamePersistentLocalId>());

            var bronAdresId = $"{AdresPuri}{existingAdddressPersistentLocalId}";
            var request = new ReaddressRequest
            {
                DoelStraatnaamId = $"{StraatNaamPuri}/1",
                HerAdresseer = new List<AddressToReaddressItem>
                {
                    new AddressToReaddressItem {BronAdresId = bronAdresId, DoelHuisnummer = "1"},
                    new AddressToReaddressItem {BronAdresId = bronAdresId, DoelHuisnummer = "2"}
                }
            };
            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                request);

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorMessage == $"Het bronAdresId zit meerdere keren in lijst van herAdresseer: {bronAdresId}."
                        && e.ErrorCode == "BronAdresIdReedsInLijstHerAdresseer"));
        }

        [Fact]
        public async Task WithDuplicateDestinationHouseNumbers_ThenThrowsValidationException()
        {
            var streamStore = MockStreamStore(streamExists: true);
            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var existingAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var existingAddressPersistentLocalId2 = new AddressPersistentLocalId(existingAddressPersistentLocalId + 1);
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAddressPersistentLocalId, streetNamePersistentLocalId);
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(existingAddressPersistentLocalId2, streetNamePersistentLocalId);

            var request = new ReaddressRequest
            {
                DoelStraatnaamId = $"{StraatNaamPuri}/1",
                HerAdresseer = new List<AddressToReaddressItem>
                {
                    new AddressToReaddressItem {BronAdresId = $"{AdresPuri}{existingAddressPersistentLocalId}", DoelHuisnummer = "1"},
                    new AddressToReaddressItem {BronAdresId = $"{AdresPuri}{existingAddressPersistentLocalId2}", DoelHuisnummer = "1"}
                }
            };
            var act = async () => await _controller.Readdress(
                new ReaddressRequestValidator(
                    new StreetNameExistsValidator(streamStore.Object),
                    backOfficeContext),
                request);

            act.Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorMessage == "Het doelHuisnummer zit meerdere keren in lijst van herAdresseer: 1."
                        && e.ErrorCode == "DoelHuisnummerReedsInLijstHerAdresseer"));
        }

        private static Mock<IStreamStore> MockStreamStore(bool streamExists)
        {
            var streamStore = new Mock<IStreamStore>();
            streamStore
                .Setup(x => x.ReadStreamBackwards(It.IsAny<StreamId>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), CancellationToken.None))
                .ReturnsAsync(new ReadStreamPage("", streamExists ? PageReadStatus.Success : PageReadStatus.StreamNotFound, 0, 0, 0, 0, ReadDirection.Backward, false));
            return streamStore;
        }
    }
}
