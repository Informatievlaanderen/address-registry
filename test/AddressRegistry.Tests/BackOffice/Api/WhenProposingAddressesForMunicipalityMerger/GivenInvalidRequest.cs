namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddressesForMunicipalityMerger
{
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldAddressId is required at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldAddressId is NaN at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("StreetNameName is required at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("HouseNumber is required at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("HouseNumber is invalid at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("BoxNumber is invalid at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("PostalCode is required at record number 1");
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
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("No streetname relation found for oldAddressId 2268196 at record number 1");
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
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldAddressPuri is not unique");
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
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("House numbers are not unique for street 'Vagevuurstraat' and ''");
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
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    dbContext,
                    backOfficeContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Box numbers are not unique for street 'Vagevuurstraat' and ''");
        }
    }
}
