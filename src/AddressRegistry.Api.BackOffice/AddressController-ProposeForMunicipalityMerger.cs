namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Consumer.Read.StreetName;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using StreetName;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Stel een adres voor.
        /// Accept a csv file with addresses and their street names.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="nisCode"></param>
        /// <param name="persistentLocalIdGenerator"></param>
        /// <param name="streetNameConsumerContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("acties/voorstellen/gemeentefusie/{niscode}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(ProposeAddressRequest), typeof(ProposeAddressRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
        public async Task<IActionResult> ProposeForMunicipalityMerger(
            IFormFile? file,
            [FromRoute(Name = "niscode")] string nisCode,
            [FromServices] IPersistentLocalIdGenerator persistentLocalIdGenerator,
            [FromServices] StreetNameConsumerContext streetNameConsumerContext,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a CSV file.");

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
                return BadRequest("Only CSV files are allowed.");

            var records = new List<CsvRecord>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                       {
                           Delimiter = ";",
                           HasHeaderRecord = true,
                           IgnoreBlankLines = true
                       }))
                {
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    var recordNr = 0;
                    while (await csv.ReadAsync())
                    {
                        recordNr++;

                        var oldStreetNamePuri = csv.GetField<string>("OUD straatnaamid");
                        var oldAddressPuri = csv.GetField<string>("OUD adresid");
                        var streetNameName = csv.GetField<string>("NIEUW straatnaam");
                        var streetNameHomonymAddition = csv.GetField<string?>("NIEUW homoniemtoevoeging");
                        var houseNumber = csv.GetField<string>("NIEUW huisnummer");
                        var boxNumber = csv.GetField<string?>("NIEUW busnummer");
                        var postalCode = csv.GetField<string>("NIEUW postcode");

                        if (string.IsNullOrWhiteSpace(oldStreetNamePuri))
                            return BadRequest($"OldStreetNamePuri is required at record number {recordNr}");

                        if (!OsloPuriValidator.TryParseIdentifier(oldStreetNamePuri, out var oldStreetNamePersistentLocalIdAsString)
                            || !int.TryParse(oldStreetNamePersistentLocalIdAsString, out var oldStreetNamePersistentLocalId))
                            return BadRequest($"OldStreetNamePuri is NaN at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(oldAddressPuri))
                            return BadRequest($"OldAddressPuri is required at record number {recordNr}");

                        if (!OsloPuriValidator.TryParseIdentifier(oldAddressPuri, out var oldAddressPersistentLocalIdAsString)
                            || !int.TryParse(oldAddressPersistentLocalIdAsString, out var oldAddressNamePersistentLocalId))
                            return BadRequest($"OldAddressPuri is NaN at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(streetNameName))
                            return BadRequest($"StreetNameName is required at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(houseNumber))
                            return BadRequest($"HouseNumber is required at record number {recordNr}");

                        if (!HouseNumber.HasValidFormat(houseNumber))
                            return BadRequest($"HouseNumber is invalid at record number {recordNr}");

                        if (!string.IsNullOrWhiteSpace(boxNumber) && !BoxNumber.HasValidFormat(boxNumber))
                            return BadRequest($"BoxNumber is invalid at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(postalCode))
                            return BadRequest($"PostalCode is required at record number {recordNr}");

                        records.Add(new CsvRecord
                        {
                            RecordNumber = recordNr,
                            OldStreetNamePersistentLocalId = oldStreetNamePersistentLocalId,
                            OldAddressPersistentLocalId = oldAddressNamePersistentLocalId,
                            StreetNameName = streetNameName.Trim(),
                            StreetNameHomonymAddition =
                                string.IsNullOrWhiteSpace(streetNameHomonymAddition) ? null : streetNameHomonymAddition.Trim(),
                            HouseNumber = houseNumber.Trim(),
                            BoxNumber = string.IsNullOrWhiteSpace(boxNumber) ? null : boxNumber.Trim(),
                            PostalCode = postalCode.Trim()
                        });
                    }
                }
            }

            if (records.Count != records.Select(x => x.OldAddressPersistentLocalId).Distinct().Count())
            {
                return BadRequest("OldAddressPuri is not unique");
            }

            var addressesByStreetNameRecords = records
                .GroupBy(x => (x.StreetNameName, x.StreetNameHomonymAddition))
                .ToDictionary(
                    x => x.Key,
                    x => x.ToList());

            var sqsRequests = new List<ProposeAddressesForMunicipalityMergerSqsRequest>();

            foreach (var (streetName, addressRecords) in addressesByStreetNameRecords)
            {
                var (streetNameName, streetNameHomonymAddition) = streetName;

                var streetNameLatestItem = await streetNameConsumerContext.StreetNameLatestItems.SingleOrDefaultAsync(
                    x =>
                        // String comparisons translate to case-insensitive checks on SQL (=desired behavior)
                        x.NisCode == nisCode
                        && x.NameDutch == streetNameName
                        && x.HomonymAdditionDutch == streetNameHomonymAddition
                        && !x.IsRemoved,
                    cancellationToken: cancellationToken);

                if (streetNameLatestItem is null)
                {
                    return BadRequest($"No streetNameLatestItem found for {nisCode}, '{streetNameName}' and '{streetNameHomonymAddition}'");
                }

                var houseNumberAddressRecords = addressRecords.Where(x => x.BoxNumber is null).ToList();
                if (houseNumberAddressRecords.Count != houseNumberAddressRecords.Select(x => x.HouseNumber).Distinct().Count())
                {
                    return BadRequest($"House numbers are not unique for street '{streetNameName}' and '{streetNameHomonymAddition}'");
                }

                var boxNumberAddressRecords = addressRecords.Where(x => x.BoxNumber is not null).ToList();
                if (boxNumberAddressRecords.Count != boxNumberAddressRecords.Select(x => new { x.HouseNumber, x.BoxNumber }).Distinct().Count())
                {
                    return BadRequest($"Box numbers are not unique for street '{streetNameName}' and '{streetNameHomonymAddition}'");
                }

                sqsRequests.Add(new ProposeAddressesForMunicipalityMergerSqsRequest(
                    streetNameLatestItem.PersistentLocalId,
                    addressRecords.Select(x => new ProposeAddressesForMunicipalityMergerSqsRequestItem(
                        x.PostalCode,
                        persistentLocalIdGenerator.GenerateNextPersistentLocalId(),
                        x.HouseNumber,
                        x.BoxNumber,
                        x.OldStreetNamePersistentLocalId,
                        x.OldAddressPersistentLocalId)).ToList(),
                    new ProvenanceData(CreateProvenance(Modification.Insert, $"Fusie {nisCode}"))));
            }

            var results = await Task.WhenAll(sqsRequests.Select(sqsRequest => _mediator.Send(sqsRequest, cancellationToken)));

            return Ok(results.Select(x =>
                x.Location
                    .ToString()
                    .Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl)));
        }
    }

    public sealed class CsvRecord
    {
        public required int RecordNumber { get; init; }
        public required int OldStreetNamePersistentLocalId { get; init; }
        public required int OldAddressPersistentLocalId { get; init; }
        public required string StreetNameName { get; init; }
        public required string? StreetNameHomonymAddition { get; init; }
        public required string HouseNumber { get; init; }
        public required string? BoxNumber { get; init; }
        public required string PostalCode { get; init; }
    }
}
