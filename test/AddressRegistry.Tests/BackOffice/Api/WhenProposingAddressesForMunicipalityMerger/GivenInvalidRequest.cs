namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddressesForMunicipalityMerger
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using Consumer.Read.StreetName.Projections;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInvalidRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void WithNoFormFile_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    null,
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Please upload a CSV file.");
        }

        [Fact]
        public void WithNoCsvFile_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("abc", "file.txt"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Only CSV files are allowed.");
        }

        [Fact]
        public void WithEmptyOldAddressPuri_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("OldAddressId is required at record number 1");
        }

        [Fact]
        public void WithInvalidOldAddressPuri_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
abc;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("OldAddressId is NaN at record number 1");
        }

        [Fact]
        public void WithEmptyStreetNameName_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
2268196;;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("StreetNameName is required at record number 1");
        }

        [Fact]
        public void WithEmptyHouseNumber_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
2268196;Vagevuurstraat;;;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("HouseNumber is required at record number 1");
        }

        [Fact]
        public void WithInvalidHouseNumber_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
2268196;Vagevuurstraat;;x;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("HouseNumber is invalid at record number 1");
        }

        [Theory]
        [InlineData("x")]
        [InlineData("TRue")]
        public void WithInvalidHouseNumberAndDisabledNumberValidation_ThenStopAtOtherError(string geenNummerValidatie)
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@$"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode;Geen nummer validatie
2268196;Vagevuurstraat;;x;;8755;{geenNummerValidatie}"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().NotContain("HouseNumber is invalid at record number 1");
        }

        [Fact]
        public void WithInvalidBoxNumber_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
2268196;Vagevuurstraat;;1;-;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("BoxNumber is invalid at record number 1");
        }

        [Theory]
        [InlineData("x")]
        [InlineData("TRue")]
        public void WithInvalidBoxNumberAndDisabledNumberValidation_ThenStopAtOtherError(string geenNummerValidatie)
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@$"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode;Geen nummer validatie
2268196;Vagevuurstraat;;1;-;8755;{geenNummerValidatie}"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().NotContain("BoxNumber is invalid at record number 1");
        }

        [Fact]
        public void WithEmptyPostalCode_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
2268196;Vagevuurstraat;;14;;"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("PostalCode is required at record number 1");
        }

        [Fact]
        public void WithUnknownStreetName_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
2268196;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    new FakeBackOfficeContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("No streetname relation found for oldAddressId 2268196 at record number 1");
        }

        [Fact]
        public async Task WithDuplicateOldAddressPuri_ThenReturnsBadRequest()
        {
            var dbContext = new FakeStreetNameConsumerContextFactory().CreateDbContext();
            dbContext.StreetNameLatestItems.Add(new StreetNameLatestItem
            {
                PersistentLocalId = 1,
                NisCode = "10000",
                NameDutch = "Vagevuurstraat",
                HomonymAdditionDutch = null
            });
            dbContext.SaveChanges();

            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var addressPersistentLocalIdOne = 2268196;

            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(addressPersistentLocalIdOne), new StreetNamePersistentLocalId(1));

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString($@"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
{addressPersistentLocalIdOne};Vagevuurstraat;;14;;8755
{addressPersistentLocalIdOne};Vagevuurstraat;;15;;8755
"),
                    "10000",
                    new FakePersistentLocalIdGenerator(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain($"OldAddressPersistentLocalId {addressPersistentLocalIdOne} is not unique");
        }

        [Fact]
        public async Task WithDuplicateHouseNumbers_ThenReturnsBadRequest()
        {
            var dbContext = new FakeStreetNameConsumerContextFactory().CreateDbContext();
            dbContext.StreetNameLatestItems.Add(new StreetNameLatestItem
            {
                PersistentLocalId = 1,
                NisCode = "10000",
                NameDutch = "Vagevuurstraat",
                HomonymAdditionDutch = null
            });
            dbContext.SaveChanges();

            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var addressPersistentLocalIdOne = 2268196;
            var addressPersistentLocalIdTwo = 2268197;

            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(addressPersistentLocalIdOne), new StreetNamePersistentLocalId(1));
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(addressPersistentLocalIdTwo), new StreetNamePersistentLocalId(1));

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@$"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
{addressPersistentLocalIdOne};Vagevuurstraat;;14;;8755
{addressPersistentLocalIdTwo};Vagevuurstraat;;14;;8755
"),
                    "10000",
                    new FakePersistentLocalIdGenerator(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("House number '14' is not unique for street (Name=Vagevuurstraat, HomonymAddition=)");
        }

        [Fact]
        public async Task WithDuplicateBoxNumbers_ThenReturnsBadRequest()
        {
            var dbContext = new FakeStreetNameConsumerContextFactory().CreateDbContext();
            dbContext.StreetNameLatestItems.Add(new StreetNameLatestItem
            {
                PersistentLocalId = 1,
                NisCode = "10000",
                NameDutch = "Vagevuurstraat",
                HomonymAdditionDutch = null
            });
            dbContext.SaveChanges();

            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var addressPersistentLocalIdOne = 2268196;
            var addressPersistentLocalIdTwo = 2268197;

            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(addressPersistentLocalIdOne), new StreetNamePersistentLocalId(1));
            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(addressPersistentLocalIdTwo), new StreetNamePersistentLocalId(1));

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@$"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
{addressPersistentLocalIdOne};Vagevuurstraat;;14;A;8755
{addressPersistentLocalIdTwo};Vagevuurstraat;;14;A;8755
"),
                    "10000",
                    new FakePersistentLocalIdGenerator(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("Box number 'A' is not unique for street (Name=Vagevuurstraat, HomonymAddition=) and house number '14'");
        }

        [Fact]
        public async Task WithBoxNumberWithoutParentAddress_ThenReturnsBadRequest()
        {
            var dbContext = new FakeStreetNameConsumerContextFactory().CreateDbContext();
            dbContext.StreetNameLatestItems.Add(new StreetNameLatestItem
            {
                PersistentLocalId = 1,
                NisCode = "10000",
                NameDutch = "Vagevuurstraat",
                HomonymAdditionDutch = null
            });
            dbContext.SaveChanges();

            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            var addressPersistentLocalIdOne = 2268196;

            await backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(addressPersistentLocalIdOne), new StreetNamePersistentLocalId(1));

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@$"
OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
{addressPersistentLocalIdOne};Vagevuurstraat;;14;A;8755
"),
                    "10000",
                    new FakePersistentLocalIdGenerator(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            var errorMessages = Xunit.Assert.IsType<List<string>>(((BadRequestObjectResult)result).Value);
            errorMessages.Should().Contain("Box number 'A' does not have a corresponding house number '14' for street 'Vagevuurstraat' at record number 1");
        }
    }
}
