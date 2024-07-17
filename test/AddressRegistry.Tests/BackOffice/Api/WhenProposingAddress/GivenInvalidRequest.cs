namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentAssertions;
    using FluentValidation;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Projections.Syndication.PostalInfo;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;
    using PositieGeometrieMethode = Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts.PositieGeometrieMethode;
    using PositieSpecificatie = Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts.PositieSpecificatie;

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
                    "bla",
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
                    CsvHelpers.CreateFormFileFromString("", "file.txt"),
                    "bla",
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
    }
}
