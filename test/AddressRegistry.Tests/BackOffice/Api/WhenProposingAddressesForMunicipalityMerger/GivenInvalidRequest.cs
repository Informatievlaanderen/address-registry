namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddressesForMunicipalityMerger
{
    using System.Threading;
    using AddressRegistry.Api.BackOffice;
    using Consumer.Read.StreetName.Projections;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
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
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Only CSV files are allowed.");
        }

        [Fact]
        public void WithEmptyOldStreetNamePuri_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldStreetNamePuri is required at record number 1");
        }

        [Fact]
        public void WithInvalidOldStreetNamePuri_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/abc;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldStreetNamePuri is NaN at record number 1");
        }

        [Fact]
        public void WithEmptyOldAddressPuri_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldAddressPuri is required at record number 1");
        }

        [Fact]
        public void WithInvalidOldAddressPuri_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/abc;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldAddressPuri is NaN at record number 1");
        }

        [Fact]
        public void WithEmptyStreetNameName_ThenReturnsBadRequest()
        {
            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
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
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
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
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;x;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
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
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;1;-;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
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
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;;"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
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
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;;8755"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    new FakeStreetNameConsumerContextFactory().CreateDbContext(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("No streetNameLatestItem found for 10000, 'Vagevuurstraat' and ''");
        }

        [Fact]
        public void WithDuplicateOldAddressPuri_ThenReturnsBadRequest()
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

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;;8755
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;15;;8755
"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    dbContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("OldAddressPuri is not unique");
        }

        [Fact]
        public void WithDuplicateHouseNumbers_ThenReturnsBadRequest()
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

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;;8755
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268197;Vagevuurstraat;;14;;8755
"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    dbContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("House numbers are not unique for street 'Vagevuurstraat' and ''");
        }

        [Fact]
        public void WithDuplicateBoxNumbers_ThenReturnsBadRequest()
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

            var result =
                _controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(@"
OUD straatnaamid;OUD adresid;NIEUW straatnaam;NIEUW homoniemtoevoeging;NIEUW huisnummer;NIEUW busnummer;NIEUW postcode
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268196;Vagevuurstraat;;14;A;8755
https://data.vlaanderen.be/id/straatnaam/59111;https://data.vlaanderen.be/id/adres/2268197;Vagevuurstraat;;14;A;8755
"),
                    "10000",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    dbContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Box numbers are not unique for street 'Vagevuurstraat' and ''");
        }
    }
}
